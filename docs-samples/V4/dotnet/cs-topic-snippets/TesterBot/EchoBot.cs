using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace TesterBot
{
    public class EchoBot : IBot
    {
        private IStatePropertyAccessor<EchoState> TesterProperties { get; }

        public EchoBot(ConversationState state, string name = null)
        {
            TesterProperties = state.CreateProperty<EchoState>($"{name ?? nameof(EchoBot)}.{nameof(TesterProperties)}");
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    var activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync("Welcome to the Echo bot!", cancellationToken: cancellationToken);
                    }

                    break;

                case ActivityTypes.Message:

                    // Get the conversation state from the turn context
                    var state = await TesterProperties.GetAsync(turnContext, () => new EchoState());

                    // Bump the turn count. 
                    state.TurnCount++;

                    // Echo back to the user whatever they typed.
                    await turnContext.SendActivityAsync($"Turn {state.TurnCount}: You sent '{turnContext.Activity.Text}'");

                    break;
            }
        }
    }
}
