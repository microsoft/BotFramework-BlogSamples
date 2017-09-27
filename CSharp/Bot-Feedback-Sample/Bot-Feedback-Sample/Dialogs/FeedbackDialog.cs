using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace Bot_Feedback_Sample
{
    [Serializable]
    public class FeedbackDialog : IDialog<IMessageActivity>
    {
        private string qnaURL;
        private string userQuestion;

        public FeedbackDialog(string url, string question)
        {
            // keep track of data associated with feedback
            qnaURL = url;
            userQuestion = question;
        }

        public Dictionary<string, string> GetFeedbackProperties()
        {
            var properties = new Dictionary<string, string>
            {
                {"Question", userQuestion },
                {"URL", qnaURL }
            };

            return properties;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var feedback = ((Activity)context.Activity).CreateReply("Did you find what you need?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(this.MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var userFeedback = await result;

            if (userFeedback.Text.Contains("yes-positive-feedback") || userFeedback.Text.Contains("no-negative-feedback"))
            {
                // create telemetry client to post to Application Insights 
                TelemetryClient telemetry = new TelemetryClient();

                if (userFeedback.Text.Contains("yes-positive-feedback"))
                {
                    // post feedback to App Insights
                    var properties = new Dictionary<string, string>
                    {
                        {"Question", userQuestion },
                        {"URL", qnaURL },
                        {"Vote", "Yes" }
                        // add properties relevant to your bot 
                    };

                    telemetry.TrackEvent("Yes-Vote", properties);
                }
                else if (userFeedback.Text.Contains("no-negative-feedback"))
                {
                    // post feedback to App Insights
                }

                await context.PostAsync("Thanks for your feedback!");

                context.Done<IMessageActivity>(null);
            }
            else
            {
                // no feedback, return to QnA dialog
                context.Done<IMessageActivity>(userFeedback);
            }
        }
    }
}



