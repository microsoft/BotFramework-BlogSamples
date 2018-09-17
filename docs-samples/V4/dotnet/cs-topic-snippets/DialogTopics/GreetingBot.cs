namespace DialogTopics
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    public class GreetingBot : IBot
    {
        private GreetingDialogSet GreetingsDialogs { get; }

        public GreetingBot(GreetingDialogSet dialogSet)
        {
            GreetingsDialogs = dialogSet;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Generate a dialog context for the addition dialog.
            Microsoft.Bot.Builder.Dialogs.DialogContext dc = await GreetingsDialogs.CreateContextAsync(turnContext);

            switch (turnContext.Activity.Type)
            {
                // Handle conversation activity from the channel.
                case ActivityTypes.ConversationUpdate:

                    IConversationUpdateActivity activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync($"Welcome to the greeting dialog bot!");
                        await dc.BeginDialogAsync(GreetingDialogSet.Main);
                    }

                    break;

                // Handle any message activity from the user.
                case ActivityTypes.Message:

                    // Continue any active dialog.
                    var turnResult = await dc.ContinueDialogAsync();
                    if (turnResult.Status == DialogTurnStatus.Complete
                        && turnResult.Result is GreetingDialogSet.Output userInfo)
                    {
                        // Do something with the result.
                        await turnContext.SendActivityAsync(
                            $"Name: {userInfo.Name}, workplace: {userInfo.WorkPlace}.");
                    }

                    if (!turnContext.Responded)
                    {
                        // Restart the dialog.
                        await turnContext.SendActivityAsync("Let's start again.");
                        await dc.BeginDialogAsync(GreetingDialogSet.Main);
                    }

                    break;
            }
        }
    }
}
