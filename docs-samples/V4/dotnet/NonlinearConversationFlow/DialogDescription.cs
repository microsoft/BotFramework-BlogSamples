using Microsoft.Bot.Builder.Dialogs;

namespace NonlinearConversationFlow
{
    public class DialogDescription
    {
        public string Name { get; set; }
        public string Description { get; set; } = null;
        public WaterfallStep[] Steps { get; set; }
    }
}
