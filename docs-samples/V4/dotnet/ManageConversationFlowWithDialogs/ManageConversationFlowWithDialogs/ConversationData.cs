using System.Collections.Generic;

namespace ManageConversationFlowWithDialogs
{
    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class ConversationData
    {
        /// <summary>Property for storing dialog state.</summary>
        public Dictionary<string, object> DialogState { get; set; } = new Dictionary<string, object>();
    }
}
