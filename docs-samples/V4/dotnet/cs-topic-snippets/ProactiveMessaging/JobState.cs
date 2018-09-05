namespace ProactiveMessaging
{
    using Microsoft.Bot.Builder;

    public class JobState : BotState
    {
        public const string ServiceKey = "ProactiveBot.JobState";

        public JobState(IStorage store) : base(store, ServiceKey) { }

        protected override string GetStorageKey(ITurnContext turnContext)
        {
            return ServiceKey;
        }
    }
}
