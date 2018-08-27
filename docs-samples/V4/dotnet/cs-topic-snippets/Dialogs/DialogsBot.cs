using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using MetaBot;
using System.Collections.Generic;
using System;
using System.Threading;
using Microsoft.Bot.Builder.TraceExtensions;
using System.Diagnostics;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Extensions.Options;
using System.Diagnostics.Contracts;

namespace Dialogs
{
    public class DialogsBot : TopicSelectionBot
    {
        private IStatePropertyAccessor<EchoState> EchoStateAccessor { get; }

        public DialogsBot(
            SelectionDialogSet selectionDialog,
            OutterStateAccessors accessors)
            : base(selectionDialog)
        {
            Contract.Requires(accessors != null);
            Contract.Requires(accessors.PropertyAccessor != null);
            EchoStateAccessor = accessors.PropertyAccessor;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await EchoStateAccessor.GetAsync(turnContext, () => new EchoState());

            await turnContext.TraceActivityAsync($"Turn {state.TurnCount}", "Entering DialogBot turn handler.");
            Debug.WriteLine($"Starting turn {state.TurnCount} ({turnContext.Activity.Type}).");

            await base.OnTurnAsync(turnContext, cancellationToken);

            Debug.WriteLine($"Finishing turn {state.TurnCount} ({turnContext.Activity.Type}).");
            state.TurnCount++;
        }
    }
}
