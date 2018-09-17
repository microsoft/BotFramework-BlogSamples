using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerLib
{
    public class ContainerBot : IBot
    {
        private ContainerDialogSet ContainerDialog { get; }

        public ContainerBot(ContainerDialogSet containerDialog)
        {
            ContainerDialog = containerDialog ?? throw new ArgumentNullException(nameof(containerDialog));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dc = await ContainerDialog.CreateContextAsync(turnContext);
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    var activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync($"Welcome to the {ContainerDialog.Name} bot!", cancellationToken: cancellationToken);
                        await dc.BeginDialogAsync(ContainerDialog.Default);
                    }

                    break;

                case ActivityTypes.Message:

                    await dc.ContinueDialogAsync();
                    if (!turnContext.Responded)
                    {
                        await turnContext.SendActivityAsync("Let's start over!", cancellationToken: cancellationToken);
                        await dc.BeginDialogAsync(ContainerDialog.Default);
                    }

                    break;
            }
        }
    }
}
