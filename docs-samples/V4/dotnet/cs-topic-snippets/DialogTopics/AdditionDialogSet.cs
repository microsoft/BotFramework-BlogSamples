using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace DialogTopics
{
    /// <summary>Defines a simple dialog for adding two numbers together.</summary>
    public class AdditionDialogSet : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Main = "addTwoNumbers";

        /// <summary>
        /// Define the input arguments to the dialog.
        /// </summary>
        public class Options : DialogOptions
        {
            public double First { get; set; }
            public double Second { get; set; }
        }

        public AdditionDialogSet(IStatePropertyAccessor<DialogState> dialogStateAccessor)
            : base(dialogStateAccessor)
        {
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    // Get the input to the dialog and add them.
                    Options options = step.Options as Options;
                    double sum = options.First + options.Second;

                    // Display the result to the user.
                    await dc.Context.SendActivityAsync($"{options.First} + {options.Second} = {sum}");

                    // End the dialog and return the sum.
                    return await dc.EndAsync(sum);
                }
            }));
        }
    }
}
