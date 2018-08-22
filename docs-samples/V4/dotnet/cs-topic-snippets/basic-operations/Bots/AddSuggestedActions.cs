namespace basicOperations
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class AddSuggestedActions : IBot
    {
        public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
        {
            // Create the activity and add suggested actions.
            IMessageActivity activity = MessageFactory.SuggestedActions(
                actions: new string[] { "red", "green", "blue" },
                text: "Choose a color");

            // Send the activity as a reply to the user.
            await context.SendActivityAsync(activity, token);
        }
    }
}
