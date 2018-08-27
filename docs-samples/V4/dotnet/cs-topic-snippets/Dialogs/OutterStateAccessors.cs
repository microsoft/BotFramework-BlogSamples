using MetaBot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dialogs
{
    public class OutterStateAccessors
    {
        public const string DialogStateName = "DialogState";

        public IStatePropertyAccessor<DialogState> DialogState { get; set; }

        /// <summary>
        /// The identifier for the state property accessor for the book-a-table dialog.
        /// </summary>
        public const string OuterBotState = "EchoBotStateKey";

        /// <summary>
        /// Gets or sets the state property accessor for the book-a-table dialog.
        /// </summary>
        public IStatePropertyAccessor<EchoState> PropertyAccessor { get; set; }
    }
}
