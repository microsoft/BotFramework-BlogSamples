using System.Collections.Generic;

namespace PromptingUsers
{
    /// <summary>
    /// Class for storing conversation state. 
    /// </summary>
    public class MyState
    {
        public string DialogNName { get; set; }

        public Dictionary<string, object> DialogState { get; set; } = new Dictionary<string, object>();
    }
}
