using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContainerLib
{
    public class MetaState : BotState
    {
        public string StorageKey { get; }

        public MetaState(IStorage storage, string name = null)
            : base(storage, string.IsNullOrWhiteSpace(name) ? typeof(MetaState).FullName : name.Trim())
        {
            StorageKey = string.IsNullOrWhiteSpace(name) ? typeof(MetaState).FullName : name.Trim();
        }

        protected override string GetStorageKey(ITurnContext turnContext)
        {
            return StorageKey;
        }
    }
}
