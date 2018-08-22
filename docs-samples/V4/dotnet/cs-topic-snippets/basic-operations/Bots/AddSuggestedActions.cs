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
                new CardAction[]
                {
                    new CardAction(title: "red", type: ActionTypes.ImBack, value: "red"),
                    new CardAction( title: "green", type: ActionTypes.ImBack, value: "green"),
                    new CardAction(title: "blue", type: ActionTypes.ImBack, value: "blue")
                },
                text: "Choose a color");

            // Send the activity as a reply to the user.
            await context.SendActivityAsync(activity, token);
        }
    }
}
