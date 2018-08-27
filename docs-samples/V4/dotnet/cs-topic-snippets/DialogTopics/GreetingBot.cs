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
    public class GreetingBot : IBot
    {
        private IStatePropertyAccessor<DialogState> DialogStateAccessor { get; }

        private GreetingDialogSet GreetingsDialogs { get; }

        public GreetingBot(IStatePropertyAccessor<DialogState> dialogStateAccessor, GreetingDialogSet dialogSet)
        {
            DialogStateAccessor = dialogStateAccessor;
            GreetingsDialogs = dialogSet;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Generate a dialog context for the addition dialog.
            var dc = await GreetingsDialogs.CreateContextAsync(turnContext);

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:

                    var activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync($"Welcome to the greeting dialog bot!");
                        await dc.BeginAsync(GreetingDialogSet.Main);
                    }

                    break;

                case ActivityTypes.Message:

                    // Handle any message activity from the user.
                    if (turnContext.Activity.Type is ActivityTypes.Message)
                    {
                        // Continue any active dialog.
                        await dc.ContinueAsync();
                        if (!turnContext.Responded)
                        {
                            // Restart the dialog.
                            await turnContext.SendActivityAsync("Let's start again.");
                            await dc.BeginAsync(GreetingDialogSet.Main);
                        }
                    }

                    break;
            }
        }
    }
}
