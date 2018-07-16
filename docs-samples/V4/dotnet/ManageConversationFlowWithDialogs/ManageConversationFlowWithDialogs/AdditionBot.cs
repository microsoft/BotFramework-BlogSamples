using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ManageConversationFlowWithDialogs
{
    public class AdditionBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            // Handle any message activity from the user.
            if (context.Activity.Type is ActivityTypes.Message)
            {
                // Get the conversation state from the turn context
                var conversationState = context.GetConversationState<ConversationData>();

                // Generate a dialog context for the addition dialog.
                var dc = AdditionDialog.Instance.CreateContext(context, conversationState.DialogState);

                // Continue any active dialog.
                await dc.Continue();

                // If no dialog is active, the bot will not have responded yet.
                if (!context.Responded)
                {
                    // Call a helper function that identifies if the user says something 
                    // like "2 + 3" or "1.25 + 3.28" and extract the numbers to add.
                    if (TryParseAddingTwoNumbers(context.Activity.Text, out double first, out double second))
                    {
                        await dc.Begin(AdditionDialog.Main, new Dictionary<string, object>
                        {
                            [AdditionDialog.Input.First] = first,
                            [AdditionDialog.Input.Second] = second
                        });
                    }
                    else
                    {
                        // Echo back to the user whatever they typed.
                        await context.SendActivity($"You said '{context.Activity.Text}'");
                    }
                }
            }
        }

        // Recognizes if the message is a request to add 2 numbers, in the form: number + number, 
        // where number may have optionally have a decimal point.: 1 + 1, 123.99 + 45, 0.4+7. 
        // For the sake of simplicity it doesn't handle negative numbers or numbers like 1,000 that contain a comma.
        // If you need more robust number recognition, try System.Recognizers.Text
        public bool TryParseAddingTwoNumbers(string message, out double first, out double second)
        {
            // captures a number with optional -/+ and optional decimal portion
            const string NUMBER_REGEXP = "([-+]?(?:[0-9]+(?:\\.[0-9]+)?|\\.[0-9]+))";

            // matches the plus sign with optional spaces before and after it
            const string PLUSSIGN_REGEXP = "(?:\\s*)\\+(?:\\s*)";

            const string ADD_TWO_NUMBERS_REGEXP = NUMBER_REGEXP + PLUSSIGN_REGEXP + NUMBER_REGEXP;

            var regex = new Regex(ADD_TWO_NUMBERS_REGEXP);
            var matches = regex.Matches(message);

            first = 0;
            second = 0;
            if (matches.Count > 0)
            {
                var matched = matches[0];
                if (double.TryParse(matched.Groups[1].Value, out first)
                    && double.TryParse(matched.Groups[2].Value, out second))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
