namespace DialogTopics
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class PromptsDialogBot : IBot
    {
        private PromptsDialogSet PromptsDialogs { get; }

        public PromptsDialogBot(PromptsDialogSet dialogSet)
        {
            PromptsDialogs = dialogSet;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Generate a dialog context for the addition dialog.
            Microsoft.Bot.Builder.Dialogs.DialogContext dc = await PromptsDialogs.CreateContextAsync(turnContext);

            switch (turnContext.Activity.Type)
            {
                // Handle conversation activity from the channel.
                case ActivityTypes.ConversationUpdate:

                    IConversationUpdateActivity activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync($"Welcome to the prompts dialog bot!");
                        await dc.BeginAsync(PromptsDialogSet.Main);
                    }

                    break;

                // Handle any message activity from the user.
                case ActivityTypes.Message:

                    // Continue any active dialog.
                    Microsoft.Bot.Builder.Dialogs.DialogTurnResult turnResult = await dc.ContinueAsync();
                    if (!turnContext.Responded)
                    {
                        // Restart the dialog.
                        await turnContext.SendActivityAsync("Let's start again.");
                        await dc.BeginAsync(PromptsDialogSet.Main);
                    }

                    break;
            }
        }
    }
}
