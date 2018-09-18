using Microsoft.Bot.Builder;
using System;

namespace PrimitivePrompts
{
    /// <summary>
    /// Contains the state and state property accessors for the primitive prompts bot.
    /// </summary>
    public class BotAccessors
    {
        public const string TopicStateName = "PrimitivePrompts.TopicStateAccessor";

        public const string UserProfileName = "PrimitivePrompts.UserProfileAccessor";

        //public BotStateSet StateSet { get; }

        public ConversationState ConversationState { get; }

        public UserState UserState { get; }

        public IStatePropertyAccessor<TopicState> TopicStateAccessor { get; set; }

        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }

        public BotAccessors(ConversationState conversationState, UserState userState)
        {
            if (conversationState is null)
            {
                throw new ArgumentNullException(nameof(conversationState));
            }

            if (userState is null)
            {
                throw new ArgumentNullException(nameof(userState));
            }

            // StateSet = new BotStateSet(conversationState, userState);

            this.ConversationState = conversationState;
            this.UserState = userState;
        }
    }
}