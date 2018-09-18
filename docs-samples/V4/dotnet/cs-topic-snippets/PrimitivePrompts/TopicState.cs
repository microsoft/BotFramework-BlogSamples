namespace PrimitivePrompts
{
    /// <summary>
    /// Contains conversation state information about the conversation in progress.
    /// </summary>
    public class TopicState
    {
        /// <summary>
        /// Identifies the current "topic" of conversation.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Indicates whether we asked the user a question last turn, and
        /// if so, what it was.
        /// </summary>
        public string Prompt { get; set; }
    }
}