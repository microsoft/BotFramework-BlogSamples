using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContainerLib
{
    public class ContainerDialogSet : DialogSet
    {
        public class StatePropertyAccessors
        {
            public IStatePropertyAccessor<DialogState> DialogState { get; set; }
        }

        private struct Inputs
        {
            public const string Main = "mainDialog";
        }

        public string Main => Inputs.Main;

        private IBot TargetBot { get; }

        public ContainerDialogSet(
//            IStatePropertyAccessor<DialogState> dialogState,
            StatePropertyAccessors accessors,
            IBot targetBot)
            : base(accessors.DialogState)
        {
            TargetBot = targetBot;

            Add(new WaterfallDialog(Inputs.Main, new WaterfallStep[]
            {
                async (dc, step) =>
                {
                    await TargetBot.OnTurnAsync(dc.Context);
                    return Dialog.EndOfTurn;
                },
                                async (dc, step) =>
                {
                    return await dc.ReplaceAsync(Inputs.Main);
                },
            }));
        }
    }
}
