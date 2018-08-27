using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerLib
{
    public class TopicSelectorBot : IBot
    {
        private TopicSelectorDialogSet TopicSelection { get; }

        public TopicSelectorBot(TopicSelectorDialogSet topicSelection)
        {
            TopicSelection = topicSelection ?? throw new ArgumentNullException(nameof(topicSelection));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dc = await TopicSelection.CreateContextAsync(turnContext);
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    var activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await turnContext.SendActivityAsync($"Welcome to {TopicSelection.Name}!", cancellationToken: cancellationToken);
                        await dc.BeginAsync(TopicSelection.Default);
                    }

                    break;

                case ActivityTypes.Message:

                    await dc.ContinueAsync();
                    if (!turnContext.Responded)
                    {
                        await turnContext.SendActivityAsync("Let's start over!", cancellationToken: cancellationToken);
                        await dc.BeginAsync(TopicSelection.Default);
                    }

                    break;
            }
        }
    }
}
