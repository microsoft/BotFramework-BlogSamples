using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Scorable.Dialogs
{
    [Serializable]
    public class JokeDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            // Confirmation that we're in the JokeDialog, forwarded from the LUIS dialog
            string response = "What time does the duck wake up? At the quack of dawn!";

            context.PostAsync(response);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
        }
    }
}