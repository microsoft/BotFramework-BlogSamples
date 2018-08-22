using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace basicOperations
{
    public class StateAccessors
    {
        public const string TopicStateName = "TopicState";
        public const string SelectionDialog = "TopicSelectionDialog";

        public IStatePropertyAccessor<TopicState> TopicState { get; set; }

        public IStatePropertyAccessor<DialogState> SelectionDialogState { get; set; }
    }
}
