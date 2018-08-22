using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReferenceBot
{
    /// <summary>
    /// Contains state property accessors for the bot.
    /// </summary>
    public class StateAccessors
    {
        /// <summary>
        /// The identifier for the state property accessor for the EchoBot.
        /// </summary>
        public const string EchoStateKey = "EchoBotStateKey";

        /// <summary>
        /// Gets or sets the state property accessor for the EchoBot.
        /// </summary>
        public IStatePropertyAccessor<EchoState> EchoStateAccessor { get; set; }

        /// <summary>
        /// The identifier for the state property accessor for the EchoBot.
        /// </summary>
        public const string DialogStateKey = "EchoBotStateKey";

        /// <summary>
        /// Gets or sets the state property accessor for the EchoBot.
        /// </summary>
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
    }
}
