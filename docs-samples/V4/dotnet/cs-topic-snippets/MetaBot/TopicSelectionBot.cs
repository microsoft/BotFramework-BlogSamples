using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Microsoft.Bot.Builder.Integration;
using System.Reflection;

namespace MetaBot
{
    public class TopicSelectionBot : IBot
    {
        /// <summary>A dialog set for navigating the topic-section-snippet structure.</summary>
        private SelectionDialogSet SelectionDialog { get; }

        /// <summary>Creates a new instance of the bot.</summary>
        /// <param name="accessor">The state property accessors for the bot.</param>
        protected TopicSelectionBot(SelectionDialogSet dialogs)
        {
            SelectionDialog = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        }

        public virtual async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            DialogContext dc = await SelectionDialog.CreateContextAsync(turnContext);
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:

                    IConversationUpdateActivity update = turnContext.Activity.AsConversationUpdateActivity();
                    if (update.MembersAdded.Any(m => m.Id != update.Recipient.Id))
                    {
                        await dc.BeginDialogAsync(SelectionDialogSet.Inputs.ChooseTopic);
                    }

                    break;

                case ActivityTypes.Message:

                    DialogTurnResult turnResult = await dc.ContinueDialogAsync();

                    break;
            }
        }
    }
}
