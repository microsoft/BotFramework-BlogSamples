using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ReferenceBot
{
    public class EchoBot : IBot
    {
        /// <summary>
        /// Gets the state property accessors for the bot.
        /// </summary>
        private StateAccessors Accessors { get; }

        private DialogSet MyDialogs { get; }

        public EchoBot(StateAccessors accessors, DialogSet dialogs)
        {
            Accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            MyDialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await MyDialogs.CreateContextAsync(turnContext);
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:

                    var activity = turnContext.Activity.AsConversationUpdateActivity();
                    if (activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                    {
                        await dialogContext.BeginAsync(MyDialogSet.Name);
                    }

                    break;

                case ActivityTypes.Message:

                    await dialogContext.ContinueAsync();

                    if (!turnContext.Responded)
                    {
                        await turnContext.SendActivityAsync("Let's begin again, shall we?");
                        await dialogContext.BeginAsync(MyDialogSet.Name);
                    }

                    break;
            }
        }
    }
}
