using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ContainerLib
{
    public class ContainerDialogSet : DialogSet, IMetaDialogSet
    {
        public class StatePropertyAccessors
        {
            public IStatePropertyAccessor<DialogState> DialogState { get; set; }
        }

        private struct Inputs
        {
            public const string Main = "mainDialog";
        }

        public string Default => Inputs.Main;

        public string Name => "a simple pass-through bot";

        private IBot TargetBot { get; }

        public ContainerDialogSet(IStatePropertyAccessor<DialogState> dialogState, IBot targetBot)
            : base(dialogState)
        {
            TargetBot = targetBot;

            Add(new WaterfallDialog(Inputs.Main, new WaterfallStep[]
            {
                async (dc, step, cancellationToken) =>
                {
                    await TargetBot.OnTurnAsync(dc.Context);
                    return Dialog.EndOfTurn;
                },
                                async (dc, step, cancellationToken) =>
                {
                    return await dc.ReplaceAsync(Inputs.Main);
                },
            }));
        }
    }
}
