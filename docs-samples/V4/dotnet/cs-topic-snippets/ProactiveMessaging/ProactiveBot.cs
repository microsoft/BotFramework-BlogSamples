namespace ProactiveMessaging
{
    using System;
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
        /// <summary>The name of events that signal that a job has completed.</summary>
        public const string JobCompleteEventName = "jobComplete";

        /// <summary>The bot's app ID.</summary>
        private string AppId { get; }

        private JobState _jobState;

        /// <summary>The state property accessor for the job log.</summary>
        private IStatePropertyAccessor<JobLog> JobLogAccessor { get; }

        /// <summary>Creates a new instance of the <see cref="ProactiveBot"/> class.</summary>
        /// <param name="configuration">The configuration properties for the app.</param>
        /// <param name="options">The Bot Framework options for the bot.</param>
        public ProactiveBot(IConfiguration configuration, IOptions<BotFrameworkOptions> options)
        {
            // Get the bot's app ID.
            AppId = configuration["MicrosoftAppId"] ?? "1";

            // Create the job log accessor off of job state middleware.
            _jobState = options.Value.State.OfType<JobState>().FirstOrDefault();
            JobLogAccessor = _jobState.CreateProperty<JobLog>("ProactiveBot.JobLog");
        }

        /// <summary>Handles an incoming activity.</summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                // Handle non-message activities.
                await OnSystemActivity(turnContext);
            }
            else
            {
                // Get the job log from the turn context.
                JobLog jobLog = await JobLogAccessor.GetAsync(turnContext, () => new JobLog());

                // Get the user's text input for the message.
                var text = turnContext.Activity.Text.Trim().ToLowerInvariant();
                switch (text)
                {
                    case "run":
                    case "run job":

                        // Start a virtual job for the user.
                        JobLog.JobData job = CreateJob(turnContext, jobLog);
                        ConversationReference conversation = turnContext.Activity.GetConversationReference();

                        await turnContext.SendActivityAsync(
                            $"We're starting job {job.TimeStamp} for you. We'll notify you when it's complete.");

                        break;

                    case "show":
                    case "show jobs":

                        // Display information for all jobs in the log.
                        if (jobLog.Count > 0)
                        {
                            await turnContext.SendActivityAsync(
                                "| Job number &nbsp; | Conversation ID &nbsp; | Completed |<br>" +
                                "| :--- | :---: | :---: |<br>" +
                                string.Join("<br>", jobLog.Values.Select(j =>
                                    $"| {j.TimeStamp} &nbsp; | {j.Conversation.Conversation.Id} &nbsp; | {j.Completed} |")));
                        }
                        else
                        {
                            await turnContext.SendActivityAsync("The job log is empty.");
                        }

                        break;

                    default:

                        // Check whether this is simulating a job completed event.
                        string[] parts = text?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts != null && parts.Length is 2
                            && parts[0].Equals("done", StringComparison.InvariantCultureIgnoreCase)
                            && long.TryParse(parts[1], out long jobNumber))
                        {
                            if (!jobLog.TryGetValue(jobNumber, out JobLog.JobData jobInfo))
                            {
                                await turnContext.SendActivityAsync($"The log does not contain a job {jobInfo.TimeStamp}.");
                            }
                            else if (jobInfo.Completed)
                            {
                                await turnContext.SendActivityAsync($"Job {jobInfo.TimeStamp} is already complete.");
                            }
                            else
                            {
                                await turnContext.SendActivityAsync($"Completing job {jobInfo.TimeStamp}.");

                                CompleteJobAsync(turnContext.Adapter, AppId, jobInfo);
                            }
                        }

                        break;
                }

                if (!turnContext.Responded)
                {
                    await turnContext.SendActivityAsync(
                        "Type `run` or `run job` to start a new job.<br>" +
                        "Type `show` or `show jobs` to display the job log.<br>" +
                        "Type `done <jobNumber>` to complete a job.");
                }

                await JobLogAccessor.SetAsync(turnContext, jobLog, cancellationToken);
                await _jobState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
        }

        /// <summary>Handles non-message activities.</summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task OnSystemActivity(ITurnContext turnContext)
        {
            // On a job completed event, mark the job as complete and notify the user.
            if (turnContext.Activity.Type is ActivityTypes.Event)
            {
                var jobLog = await JobLogAccessor.GetAsync(turnContext, () => new JobLog());
                var activity = turnContext.Activity.AsEventActivity();
                if (activity.Name is JobCompleteEventName
                    && activity.Value is long timestamp
                    && jobLog.ContainsKey(timestamp)
                    && !jobLog[timestamp].Completed)
                {
                    CompleteJobAsync(turnContext.Adapter, AppId, jobLog[timestamp]);
                }
            }
        }

        /// <summary>Creates and "starts" a new job.</summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="jobLog">A log of all the jobs.</param>
        /// <returns>The information for the created job.</returns>
        private JobLog.JobData CreateJob(ITurnContext turnContext, JobLog jobLog)
        {
            JobLog.JobData jobInfo = new JobLog.JobData
            {
                TimeStamp = DateTime.Now.ToBinary(),
                Conversation = turnContext.Activity.GetConversationReference(),
            };

            jobLog[jobInfo.TimeStamp] = jobInfo;

            return jobInfo;
        }

        /// <summary>Sends a proactive message to the user.</summary>
        /// <param name="adapter">The adapter for the bot.</param>
        /// <param name="botId">The bot's app ID.</param>
        /// <param name="jobInfo">Information about the job to complete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task CompleteJobAsync(
            BotAdapter adapter,
            string botId,
            JobLog.JobData jobInfo,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await adapter.ContinueConversationAsync(botId, jobInfo.Conversation, CreateCallback(jobInfo), cancellationToken);
        }

        /// <summary>Creates the turn logic to use for the proactive message.</summary>
        /// <param name="jobInfo">Information about the job to complete.</param>
        /// <returns>The turn logic to use for the proactive message.</returns>
        private BotCallbackHandler CreateCallback(JobLog.JobData jobInfo)
        {
            return async (turnContext, token) =>
            {
                // Get the job log from state, and retrieve the job.
                JobLog jobLog = await JobLogAccessor.GetAsync(turnContext, () => new JobLog());

                // Perform bookkeeping.
                jobLog[jobInfo.TimeStamp].Completed = true;

                // Send the user a proactive confirmation message.
                await turnContext.SendActivityAsync($"Job {jobInfo.TimeStamp} is complete.");

                await JobLogAccessor.SetAsync(turnContext, jobLog, token);
                await _jobState.SaveChangesAsync(turnContext, false, token);
            };
        }
    }
}