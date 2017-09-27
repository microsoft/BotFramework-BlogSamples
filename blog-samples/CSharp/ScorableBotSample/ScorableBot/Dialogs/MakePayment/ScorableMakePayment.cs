using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

namespace ScorableTest.Dialogs.MakePayment
{
    public class ScorableMakePayment : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogStack stack;

        public ScorableMakePayment(IDialogStack stack)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return state != null && state == "scorable1-triggered" ? 1 : 0;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null && state == "scorable1-triggered";
        }

        protected override Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var message = item as IMessageActivity;
            var dialog = new ScorableMakePaymentDialog();
            var interruption = dialog.Void(stack);
            stack.Call(interruption, null);
            return Task.CompletedTask;
        }

        protected override async Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item.AsMessageActivity();
            if (message == null)
                return null;

            var messageText = message.Text;

            return messageText == "make payment" ? "scorable1-triggered" : null; // this value is passed to GetScore/HasScore/PostAsync and can be anything meaningful to the scoring
        }
    }
}