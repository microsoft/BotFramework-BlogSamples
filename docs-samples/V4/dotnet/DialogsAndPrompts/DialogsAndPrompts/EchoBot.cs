using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace ReferenceBot
{
    public class EchoBot : IBot
    {
        /// <summary>
        /// Gets the state property accessors for the bot.
        /// </summary>
        private StateAccessors Accessors { get; }

        private MyDialogSet MyDialogs { get; }

        public EchoBot(StateAccessors accessors)
        {
            Accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            MyDialogs = new MyDialogSet(Accessors.DialogStateAccessor);
        }

        /// <summary>
        /// Every Conversation turn for our EchoBot will call this method. In here
        /// the bot checks the Activty type to verify it's a message, bumps the 
        /// turn conversation 'Turn' count, and then echoes the users typing
        /// back to them. 
        /// </summary>
        /// <param name="turnContext">Turn scoped context containing all the data needed
        /// for processing this conversation turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
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
