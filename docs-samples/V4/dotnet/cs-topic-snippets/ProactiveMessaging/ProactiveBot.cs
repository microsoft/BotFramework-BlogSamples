namespace ProactiveMessaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public class ProactiveBot : IBot
    {
        private IReadOnlyCollection<string> RunPhrases { get; }
            = new List<string> { "run", "run job" };

        private string BotId { get; }

        private IStatePropertyAccessor<JobLog> JobLogAccessor { get; }

        public ProactiveBot(IConfiguration configuration, IOptions<BotFrameworkOptions> options)
        {
            BotId = configuration["MicrosoftAppId"];

            JobState jobState = options.Value.Middleware.OfType<JobState>().FirstOrDefault();
            JobLogAccessor = jobState.CreateProperty<JobLog>("ProactiveBot.JobLog");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type is ActivityTypes.Message)
            {
                JobLog jobLog = await JobLogAccessor.GetAsync(turnContext, () => new JobLog());
                string text = turnContext.Activity.AsMessageActivity()?.Text?.Trim();
                if (RunPhrases.Any(phrase => phrase.Equals(text, StringComparison.InvariantCultureIgnoreCase)))
                {
                    JobLog.JobInfo job = CreateJob(turnContext, jobLog);
                    ConversationReference conversation = turnContext.Activity.GetConversationReference();

                    await turnContext.SendActivityAsync(
                        $"We're starting job {job.JobNumber} for you. We'll notify you when it's complete.");
                }
                else if (text.StartsWith("show", StringComparison.InvariantCultureIgnoreCase))
                {
                    await turnContext.SendActivityAsync(
                        "| Job number | Completed |<br>" +
                        "| :--- | :--- |<br>" +
                        $"{string.Join("<br>", jobLog.Values.Select(j => $"| {j.JobNumber} | {j.Completed} |"))}");
                }
                else
                {
                    string[] parts = text?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts != null && parts.Length is 2
                        && parts[0].Equals("done", StringComparison.InvariantCultureIgnoreCase)
                        && long.TryParse(parts[1], out long jobNumber)
                        && jobLog.TryGetValue(jobNumber, out JobLog.JobInfo jobInfo))
                    {
                        CompleteJobAsync(turnContext.Adapter, BotId, jobInfo);
                    }
                }

                if (!turnContext.Responded)
                {
                    await turnContext.SendActivityAsync(
                        "Type `run` or `run job` to start a new job.<br>" +
                        "Type `done <jobNumber>` to complete a job.");
                }
            }
        }

        private JobLog.JobInfo CreateJob(ITurnContext turnContext, JobLog jobLog)
        {
            JobLog.JobInfo jobInfo = new JobLog.JobInfo
            {
                JobNumber = DateTime.Now.ToBinary(),
                Conversation = turnContext.Activity.GetConversationReference(),
            };

            jobLog[jobInfo.JobNumber] = jobInfo;

            return jobInfo;
        }

        private async Task CompleteJobAsync(
            BotAdapter adapter,
            string botId,
            JobLog.JobInfo jobInfo,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await adapter.ContinueConversationAsync(botId, jobInfo.Conversation, async (turnContext, token) =>
            {
                // Get the job log from state, and retrieve the job.
                JobLog jobLog = await JobLogAccessor.GetAsync(turnContext, () => new JobLog());
                if (!jobLog.ContainsKey(jobInfo.JobNumber))
                {
                    await turnContext.SendActivityAsync($"The log does not contain a job {jobInfo.JobNumber}.");
                }
                else if (jobLog[jobInfo.JobNumber].Completed)
                {
                    await turnContext.SendActivityAsync($"Job {jobInfo.JobNumber} is already complete.");
                }

                // Perform bookkeeping.
                jobLog[jobInfo.JobNumber].Completed = true;

                // Send the user a proactive confirmation message.
                await turnContext.SendActivityAsync($"Job {jobInfo.JobNumber} is complete.");
            }, cancellationToken);
        }
    }
}