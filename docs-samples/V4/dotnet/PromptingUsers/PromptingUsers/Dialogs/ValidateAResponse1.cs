using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using PromptStatus = Microsoft.Bot.Builder.Prompts.PromptStatus;

namespace ValidateAPromptResponse1
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
            Add(Inputs.Size, new NumberPrompt<int>(Culture.English, async (context, result) =>
            {
                if (result.Value < 6 || result.Value > 20)
                {
                    result.Status = PromptStatus.OutOfRange;
                }
            }));
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
    }
}
