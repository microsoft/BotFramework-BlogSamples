namespace ProactiveMessaging
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;

    public class JobLog : Dictionary<long, JobLog.JobData>
    {
        /// <summary>Describes the state of a job.</summary>
        public class JobData
        {
            /// <summary>The time-stamp for the job.</summary>
            public long TimeStamp { get; set; } = 0;

            /// <summary>Indicates whether the job has completed.</summary>
            public bool Completed { get; set; } = false;

            /// <summary>
            /// The conversation reference to which to send status updates.
            /// </summary>
            public ConversationReference Conversation { get; set; }
        }
    }
}