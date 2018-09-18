namespace PrimitivePrompts
{
    /// <summary>
    /// Contains conversation state information about the dialog in progress.
    /// </summary>
    public class DialogState
    {
        public string Topic { get; set; } = PrimitivePromptsBot.ProfileTopic;

        public string Prompt { get; set; } = null;
    }
}