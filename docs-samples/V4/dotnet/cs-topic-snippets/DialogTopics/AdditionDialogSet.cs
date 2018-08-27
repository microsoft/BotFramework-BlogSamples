using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace DialogTopics
{
    /// <summary>Defines a simple dialog for adding two numbers together.</summary>
    public class AdditionDialogSet : DialogSet
    {
        /// <summary>
        /// Define the input arguments to the dialog.
        /// </summary>
        public class Options : DialogOptions
        {
            public double First { get; set; }
            public double Second { get; set; }
        }

        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Main = "additionDialog";

        public AdditionDialogSet(IStatePropertyAccessor<DialogState> dialogStateAccessor)
            : base(dialogStateAccessor)
        {
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                async (dc, step) =>
                {
                    // Get the input from the arguments to the dialog and add them.
                    var options = step.Options as Options;
                    var sum = options.First + options.Second;

                    // Display the result to the user.
                    await dc.Context.SendActivityAsync($"{options.First} + {options.Second} = {sum}");

                    // End the dialog.
                    return await dc.EndAsync();
                }
            }));
        }
    }
}
