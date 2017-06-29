using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ScorableTest.Dialogs
{
    [Serializable]
    public class ScorableMakePaymentDialog : IDialog<object>
    {
        protected string payee;
        protected string amount;

        // Entry point to the Dialog
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"[ScorableMakePaymentDialog] Who would you like to pay?");

            // State transition - wait for 'payee' message from user
            context.Wait(MessageReceivedPayee);
        }

        public async Task MessageReceivedPayee(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            this.payee = message.Text;

            await context.PostAsync($"[ScorableMakePaymentDialog] {this.payee}, got it{Environment.NewLine}How much should I pay?");

            // State transition - wait for 'amount' message from user
            context.Wait(MessageReceivedAmount);
        }

        public async Task MessageReceivedAmount(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            this.amount = message.Text;

            await context.PostAsync($"[ScorableMakePaymentDialog] Thank you, I've paid {this.amount} to {this.payee} 💸");

            // State transition - complete this Dialog and remove it from the stack
            context.Done<object>(new object());
        }
    }
}