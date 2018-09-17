using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DialogTopics
{
    public class HotelDialogBot : IBot
    {
        private HotelDialogSet HotelDialogs { get; }

        public HotelDialogBot(HotelDialogSet dialogSet)
        {
            HotelDialogs = dialogSet;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Generate a dialog context for the addition dialog.
            Microsoft.Bot.Builder.Dialogs.DialogContext dc = await HotelDialogs.CreateContextAsync(turnContext);

            switch (turnContext.Activity.Type)
            {
                // Handle conversation activity from the channel.
                case ActivityTypes.ConversationUpdate:

                    IConversationUpdateActivity activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync($"Welcome to the Contoso Hotel dialog bot!");
                        await dc.BeginDialogAsync(HotelDialogSet.MainMenu);
                    }

                    break;

                // Handle any message activity from the user.
                case ActivityTypes.Message:

                    // Continue any active dialog.
                    DialogTurnResult turnResult = await dc.ContinueDialogAsync();
                    if (!turnContext.Responded)
                    {
                        // Not all channels send a conversationUpdate activity.
                        await dc.BeginDialogAsync(HotelDialogSet.MainMenu);
                    }

                    break;
            }
        }
    }
}
