using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Scorable.Dialogs
{
    public class CommonResponsesDialog : IDialog<object>
    {
        private readonly string _messageToSend;
        private Activity _activity;

        public CommonResponsesDialog(string message)
        {
            _messageToSend = message;
        }

        // overload constructor to handle activities (card attachments)
        public CommonResponsesDialog(Activity activity)
        {
            var heroCard = new HeroCard
            {
                Title = "Help",
                Text = "Need assisstance?",
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.OpenUrl, "Contact Us", value: "https://stackoverflow.com/questions/tagged/botframework"),
                    new CardAction(ActionTypes.OpenUrl, "FAQ", value: "https://docs.microsoft.com/bot-framework")
                }
            };
            var reply = activity.CreateReply();

            reply.Attachments.Add(heroCard.ToAttachment());

            _activity = reply;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (string.IsNullOrEmpty(_messageToSend))
            {
                await context.PostAsync(_activity);
            }
            else
            {
                await context.PostAsync(_messageToSend);
            }
            context.Done<object>(null);
        }
    }
}