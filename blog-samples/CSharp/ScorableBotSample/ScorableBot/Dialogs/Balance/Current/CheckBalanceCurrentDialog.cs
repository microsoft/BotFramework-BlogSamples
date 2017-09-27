using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;

namespace ScorableTest.Dialogs.Balance.Current
{
    [Serializable]
    public class CheckBalanceCurrentDialog : IDialog<object>
    {
        // Entry point to the Dialog
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("[CheckBalanceCurrentDialog] Your current account balance is £2000");

            // State transition - complete this Dialog and remove it from the stack
            context.Done<object>(new object());
        }
    }
}