using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dialogs
{
    /// <summary>Defines a simple dialog for adding two numbers together.</summary>
    public class AdditionDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Main = "additionDialog";

        /// <summary>Defines the IDs of the input arguments.</summary>
        public struct Inputs
        {
            public const string First = "first";
            public const string Second = "second";
        }

        public class Options
        {
            public double First { get; set; }
            public double Second { get; set; }
        }

        public AdditionDialog(IStatePropertyAccessor<DialogState> dialogState)
            : base(dialogState)
        {
            Add(new WaterfallDialog(Main, new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    // Get the input from the arguments to the dialog and add them.
                    Options options = step.Options as Options;
                    double sum = options.First + options.Second;

                    // Display the result to the user.
                    await step.Context.SendActivityAsync($"{options.First} + {options.Second} = {sum}");

                    // End the dialog.
                    return await step.EndAsync(sum);
                }
            }));
        }
    }
}
