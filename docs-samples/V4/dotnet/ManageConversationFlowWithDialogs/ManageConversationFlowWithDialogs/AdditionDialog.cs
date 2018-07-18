// <usingStatement>
using Microsoft.Bot.Builder.Dialogs;
// </usingStatement>
using System;

namespace ManageConversationFlowWithDialogs
{
    // <singleStepDialog>
    public class AdditionDialog : DialogSet
    {
        public struct Input
        {
            public const string First = "first";
            public const string Second = "second";
        }

        public struct State
        {
            public const string Value = "value";
        }

        public const string Main = "additionDialog";

        public static AdditionDialog Instance = new Lazy<AdditionDialog>(() => new AdditionDialog()).Value;

        private AdditionDialog()
        {
            Add(Main, new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    var x =(double)args[Input.First];
                    var y =(double)args[Input.Second];
                    var sum = x + y;

                    await dc.Context.SendActivity($"{x} + {y} = {sum}").ConfigureAwait(false);

                    dc.ActiveDialog.State[State.Value] = sum;

                    await dc.End().ConfigureAwait(false);
                }
            });
        }
    }
    // </singleStepDialog>
}
