namespace NonlinearConversationFlow
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class EchoState
    {
        /// <summary>Property for storing dialog state.</summary>
        public Dictionary<string, object> DialogState { get; set; } = new Dictionary<string, object>();
    }
}
