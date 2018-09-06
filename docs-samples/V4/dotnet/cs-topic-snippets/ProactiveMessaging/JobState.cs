namespace ProactiveMessaging
{
    using Microsoft.Bot.Builder;

    /// <summary>Middleware for managing bot state for "bot jobs".</summary>
    /// <remarks>This is independent from both user and conversation state because
    /// the process of running the jobs and notifying the user interacts with the
    /// bot as a distinct user on a separate conversation.</remarks>
    public class JobState : BotState
    {
        /// <summary>The key used to cache the state information in the turn context.</summary>
        private const string StorageKey = "ProactiveBot.JobState";

        /// <summary>Initializes a new instance of the job state middleware.</summary>
        /// <param name="storage">The storage provider to use.</param>
        public JobState(IStorage store) : base(store, StorageKey) { }

        /// <summary>Gets the storage key for caching state information.</summary>
        protected override string GetStorageKey(ITurnContext turnContext) => StorageKey;
    }
}
