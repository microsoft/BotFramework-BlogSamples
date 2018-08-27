using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerLib
{
    public class ContainerBot : IBot
    {
        private ContainerDialogSet Container { get; }

        public ContainerBot(ContainerDialogSet container)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dc = await Container.CreateContextAsync(turnContext);
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    var activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync("Welcome to the Passthrough bot!", cancellationToken: cancellationToken);
                        await dc.BeginAsync(Container.Main);
                    }

                    break;

                case ActivityTypes.Message:

                    await dc.ContinueAsync();
                    if (!turnContext.Responded)
                    {
                        await turnContext.SendActivityAsync("Let's start over!", cancellationToken: cancellationToken);
                        await dc.BeginAsync(Container.Main);
                    }

                    break;
            }
        }
    }
}
