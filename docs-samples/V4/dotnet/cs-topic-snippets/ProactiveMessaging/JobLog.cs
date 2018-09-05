namespace ProactiveMessaging
{
    using Microsoft.Bot.Schema;
    using System.Collections.Generic;

    public class JobLog : Dictionary<long, JobLog.JobInfo>
    {
        /// <summary>
        /// Class for storing job state. 
        /// </summary>
        public class JobInfo
        {
            public long JobNumber { get; set; } = 0;

            public bool Completed { get; set; } = false;

            /// <summary>
            /// The conversation reference to which to send status updates.
            /// </summary>
            public ConversationReference Conversation { get; set; }
        }
    }
}