using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace basicOperations
{
    public class SendMessages
    {
        public class SimpleText : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                await context.SendActivityAsync("Greetings from sample message.", cancellationToken: token);
            }
        }
        public class SpokenMessage : IBot
        {
            public async Task OnTurnAsync(ITurnContext context, CancellationToken token = default(CancellationToken))
            {
                await context.SendActivityAsync(
                    "This is text to display.",
                    speak: "This is text to speak.",
                    cancellationToken: token);
            }
        }
    }
}
