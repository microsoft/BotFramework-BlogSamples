namespace DialogTopics
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    public class AdditionBot : IBot
    {
        private AdditionDialogSet AdditionDialog { get; }

        public AdditionBot(AdditionDialogSet dialogSet)
        {
            AdditionDialog = dialogSet;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Generate a dialog context for the addition dialog.
            Microsoft.Bot.Builder.Dialogs.DialogContext dc = await AdditionDialog.CreateContextAsync(turnContext);

            switch (turnContext.Activity.Type)
            {
                // Handle conversation activity from the channel.
                case ActivityTypes.ConversationUpdate:

                    IConversationUpdateActivity activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync($"Welcome to the addition dialog bot!");
                    }

                    break;

                // Handle any message activity from the user.
                case ActivityTypes.Message:

                    // Call a helper function that identifies if the user says something
                    // like "2 + 3" or "1.25 + 3.28" and extract the numbers to add.
                    if (TryParseAddingTwoNumbers(turnContext.Activity.Text, out double first, out double second))
                    {
                        // Start the dialog, passing in the numbers to add.
                        var turnResult = await dc.BeginAsync(AdditionDialogSet.Main, new AdditionDialogSet.Options
                        {
                            First = first,
                            Second = second,
                        });
                        if (turnResult.Status == DialogTurnStatus.Complete
                            && turnResult.Result is double sum)
                        {
                            // Do something with the result.
                        }
                    }
                    else
                    {
                        // Echo back to the user whatever they typed.
                        await turnContext.SendActivityAsync($"You said '{turnContext.Activity.Text}'");
                    }

                    break;
            }
        }

        // Recognizes if the message is a request to add 2 numbers, in the form: number + number,
        // where number may have optionally have a decimal point.: 1 + 1, 123.99 + 45, 0.4+7.
        // For the sake of simplicity it doesn't handle negative numbers or numbers like 1,000 that contain a comma.
        // If you need more robust number recognition, try System.Recognizers.Text
        public static bool TryParseAddingTwoNumbers(string message, out double first, out double second)
        {
            // captures a number with optional -/+ and optional decimal portion
            const string NUMBER_REGEXP = "([-+]?(?:[0-9]+(?:\\.[0-9]+)?|\\.[0-9]+))";

            // matches the plus sign with optional spaces before and after it
            const string PLUSSIGN_REGEXP = "(?:\\s*)\\+(?:\\s*)";

            const string ADD_TWO_NUMBERS_REGEXP = NUMBER_REGEXP + PLUSSIGN_REGEXP + NUMBER_REGEXP;

            Regex regex = new Regex(ADD_TWO_NUMBERS_REGEXP);
            MatchCollection matches = regex.Matches(message);

            first = 0;
            second = 0;
            if (matches.Count > 0)
            {
                Match matched = matches[0];
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
