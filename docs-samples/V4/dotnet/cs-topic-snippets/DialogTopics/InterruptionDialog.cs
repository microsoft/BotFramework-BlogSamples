using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DialogTopics
{
    public class TheDialogs
    {
        public class SimpleConversationFlow
        {
            public class BasicDialogSet : DialogSet
            {
                public BasicDialogSet(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
                {
                }
            }

            /// <summary>Defines a simple dialog for adding two numbers together.</summary>
            public class AdditionDialog : DialogSet
            {
                /// <summary>
                /// Define the input arguments to the dialog.
                /// </summary>
                public class Options
                {
                    public double First { get; set; }
                    public double Second { get; set; }
                }

                /// <summary>The ID of the main dialog in the set.</summary>
                public const string Main = "additionDialog";

                public AdditionDialog(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
                {
                    Add(new WaterfallDialog(Main, new WaterfallStep[]
                    {
                        async (step, cancellationToken) =>
                        {
                            // Get the input from the arguments to the dialog and add them.
                            var options = step.Options as Options;
                            var sum = options.First + options.Second;

                            // Display the result to the user.
                            await step.Context.SendActivityAsync($"{options.First} + {options.Second} = {sum}");

                            // End the dialog.
                            return await step.EndDialogAsync();
                        }
                    }));
                }
            }
        }

        public class InterrutionDialog : DialogSet
        {
            public InterrutionDialog(IStatePropertyAccessor<DialogState> dialogState) : base(dialogState)
            {
            }
        }
    }
}
