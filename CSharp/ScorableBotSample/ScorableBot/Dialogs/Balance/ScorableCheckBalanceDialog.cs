using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using ScorableTest.Dialogs.Balance.Current;
using ScorableTest.Dialogs.Balance.Savings;

namespace ScorableTest.Dialogs.Balance
{
    [Serializable]
    public class ScorableCheckBalanceDialog : IDialog<object>
    {
        // Entry point to the Dialog
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("[ScorableCheckBalanceDialog] Which account - Current or Savings?");

            context.Wait(MessageReceivedOperationChoice);
        }

        public async Task MessageReceivedOperationChoice(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Text.Equals("current", StringComparison.InvariantCultureIgnoreCase))
            {
                // State transition - add 'current account' Dialog to the stack, when done call AfterChildDialogIsDone callback
                context.Call<object>(new CheckBalanceCurrentDialog(), AfterChildDialogIsDone);
            }
            else if (message.Text.Equals("savings", StringComparison.InvariantCultureIgnoreCase))
            {
                // State transition - add 'savings account' Dialog to the stack, when done call AfterChildDialogIsDone callback
                context.Call<object>(new CheckBalanceSavingsDialog(), AfterChildDialogIsDone);
            }
            else
            {
                await context.PostAsync("[ScorableCheckBalanceDialog] Please repeat, which account - Current or Savings?");

                // State transition - wait for 'operation choice' message from user (loop back)
                context.Wait(MessageReceivedOperationChoice);
            }
        }

        private async Task AfterChildDialogIsDone(IDialogContext context, IAwaitable<object> result)
        {
            // State transition - complete this Dialog and remove it from the stack
            context.Done<object>(new object());
        }
    }
}