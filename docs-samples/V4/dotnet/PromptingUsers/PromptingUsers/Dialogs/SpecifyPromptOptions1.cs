using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;

namespace SpecifyPromptOptions1
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
            public const string Size = "sizePrompt";
        }

        /// <summary>Defines the prompts and steps of the dialog.</summary>
        public MyDialog()
        {
            Add(Inputs.Size, new NumberPrompt<int>(Culture.English));
            Add(Name, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    // Prompt for the user's name.
                    await dc.Prompt(Inputs.Size, "How many people are in your party?", new PromptOptions()
                    {
                        RetryPromptString = "Sorry, please specify the number of people in your party."
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
