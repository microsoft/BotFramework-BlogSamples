using Microsoft.Bot.Builder.Dialogs;
using System;

namespace ManageConversationFlowWithDialogs
{
    /// <summary>Defines a simple dialog for adding two numbers together.</summary>
    public class AdditionDialog : DialogSet
    {
        /// <summary>The ID of the main dialog in the set.</summary>
        public const string Main = "additionDialog";

        /// <summary>Defines the IDs of the prompts in the set.</summary>
        public struct Inputs
        {
            public const string First = "first";
            public const string Second = "second";
        }

        /// <summary>Defines IDs for output from the dialog.</summary>
        public struct State
        {
            public const string Value = "value";
        }

        /// <summary>Defines the steps of the dialog.</summary>
        public AdditionDialog()
        {
            Add(Main, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    // Get the input from the arguments to the dialog and add them.
                    var x =(double)args[Inputs.First];
                    var y =(double)args[Inputs.Second];
                    var sum = x + y;

                    // Display the result to the user.
                    await dc.Context.SendActivity($"{x} + {y} = {sum}").ConfigureAwait(false);

                    // Update the dialog state with the result.
                    dc.ActiveDialog.State[State.Value] = sum;

                    // End the dialog.
                    await dc.End().ConfigureAwait(false);
                }
            });
        }
    }
}
