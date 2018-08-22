namespace basicOperations
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    public class AddInputHints
    {
        public class AcceptingInput : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                await context.SendActivityAsync(
                    textReplyToSend: "This is the text that will be displayed.",
                    speak: "This is the text that will be spoken.",
                    inputHint: InputHints.AcceptingInput,
                    cancellationToken: token);
            }
        }

        public class ExpectingInput : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                await context.SendActivityAsync(
                    textReplyToSend: "This is the text that will be displayed.",
                    speak: "This is the text that will be spoken.",
                    inputHint: InputHints.ExpectingInput,
                    cancellationToken: token);
            }
        }

        public class IgnoringInput : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                await context.SendActivityAsync(
                    textReplyToSend: "This is the text that will be displayed.",
                    speak: "This is the text that will be spoken.",
                    inputHint: InputHints.IgnoringInput,
                    cancellationToken: token);
            }
        }
    }
}
