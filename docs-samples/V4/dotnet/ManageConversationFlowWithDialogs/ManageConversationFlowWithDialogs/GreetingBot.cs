using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace ManageConversationFlowWithDialogs
{
    public class GreetingBot : IBot
    {
        private static GreetingDialog Greeting { get; } = new GreetingDialog();

        public async Task OnTurn(ITurnContext context)
        {
            // Handle any message activity from the user.
            if (context.Activity.Type is ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                var conversationState = context.GetConversationState<ConversationData>();

                // Generate a dialog context for the greeting dialog.
                var dc = Greeting.CreateContext(context, conversationState.DialogState);

                // Continue any active dialog.
                await dc.Continue().ConfigureAwait(false);

                // If no dialog is active, the bot will not have responded yet.
                if (!context.Responded)
                {
                    // Start the greeting dialog.
                    await dc.Begin(GreetingDialog.Main).ConfigureAwait(false);
                }
            }
        }
    }    
}
