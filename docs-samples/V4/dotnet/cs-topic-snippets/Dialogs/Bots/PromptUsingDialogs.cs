using Microsoft.Bot.Builder;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dialogs
{
    public class PromptUsingDialogs
    {
        public class PromptTheUser : IBot
        {
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }
        }
        public class ReusablePrompts : IBot
        {
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }
        }
    }
}
