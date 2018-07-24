using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using System.Threading.Tasks;
using Int32Result = Microsoft.Bot.Builder.Prompts.NumberResult<int>;
using PromptStatus = Microsoft.Bot.Builder.Prompts.PromptStatus;

namespace ValidateAPromptResponse2
{

    /// <summary>Defines a dialog that asks for the number of people in a party.</summary>
    public class MyDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Name = "mainDialog";

        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            /// <summary>The ID of the party size prompt.</summary>
            public const string Size = "parytySize";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        public MyDialog()
        {
            // Include a validation function for the party size prompt.
            Add(Inputs.Size, new NumberPrompt<int>(Culture.English, PartySizeValidator));
            Add(Name, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    // Prompt for the party size.
                    await dc.Prompt(Inputs.Size, "How many people are in your party?", new PromptOptions()
                    {
                        RetryPromptString = "Please specify party size between 6 and 20."
                    }).ConfigureAwait(false);
                },
                async(dc, args, next) =>
                {
                    var size = (int)args["Value"];

                    await dc.Context.SendActivity($"Okay, {size} people!").ConfigureAwait(false);
                    await dc.End().ConfigureAwait(false);
                }
            });
        }

        /// <summary>Validates input for the partySize prompt.</summary>
        /// <param name="context">The context object for the current turn of the bot.</param>
        /// <param name="result">The recognition result from the prompt.</param>
        /// <returns>An updated recognition result.</returns>
        private static async Task PartySizeValidator(ITurnContext context, Int32Result result)
        {
            if (result.Value < 6 || result.Value > 20)
            {
                result.Status = PromptStatus.OutOfRange;
            }
        }
    }
}
