using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Newtonsoft.Json.Linq;

namespace Bot_Feedback_Sample.Dialogs
{
    [Serializable]
    public class QnaDialog : QnAMakerDialog
    {
        public QnaDialog() : base(new QnAMakerService(new QnAMakerAttribute(ConfigurationManager.AppSettings["QnaSubscriptionKey"], ConfigurationManager.AppSettings["QnaKnowledgebaseId"], "Sorry, I couldn't find an answer for that", 0.5)))
        {
        }

        protected override async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {
            // answer is a string
            var answer = result.Answers.First().Answer;

            Activity reply = ((Activity)context.Activity).CreateReply();

            string[] qnaAnswerData = answer.Split(';');
            int dataSize = qnaAnswerData.Length;

            string title = qnaAnswerData[0];
            string description = qnaAnswerData[1];
            string url = qnaAnswerData[2];
            string imageURL = qnaAnswerData[3];

            HeroCard card = new HeroCard
            {
                Title = title,
                Subtitle = description,
            };

            card.Buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
            };

            card.Images = new List<CardImage>
            {
                new CardImage( url = imageURL)
            };

            reply.Attachments.Add(card.ToAttachment());


            await context.PostAsync(reply);
        }

        protected override async Task DefaultWaitNextMessageAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {
                // get the URL
                var answer = result.Answers.First().Answer;
                string[] qnaAnswerData = answer.Split(';');
                string qnaURL = qnaAnswerData[2];

                // pass user's question
                var userQuestion = (context.Activity as Activity).Text;

                context.Call(new FeedbackDialog(qnaURL, userQuestion), ResumeAfterFeedback);
            
        }

        private async Task ResumeAfterFeedback(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            if(await result != null)
            {
                await MessageReceivedAsync(context, result);
            }
            else
            {
                context.Done<IMessageActivity>(null);
            }
        }

    }
}