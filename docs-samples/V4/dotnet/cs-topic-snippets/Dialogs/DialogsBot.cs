using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using MetaBot;
using System.Collections.Generic;

namespace Dialogs
{
    public class DialogsBot : BaseMetaBot
    {
        protected override IReadOnlyList<Topic> Topics { get; }
            = new List<Topic> { };

        public DialogsBot(StateAccessors accessors) : base(accessors) { }
    }
}
