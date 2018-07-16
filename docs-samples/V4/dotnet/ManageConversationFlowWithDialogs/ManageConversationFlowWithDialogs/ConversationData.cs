using System.Collections.Generic;

namespace ManageConversationFlowWithDialogs
{
    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class ConversationData
    {
        public Dictionary<string, object> DialogState { get; set; } = new Dictionary<string, object>();
    }
}
