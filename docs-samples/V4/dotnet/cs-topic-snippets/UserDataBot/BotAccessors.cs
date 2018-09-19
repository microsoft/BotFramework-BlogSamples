namespace UserDataBot
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using System;

    public class BotAccessors
    {
        public UserState UserState { get; }

        public ConversationState ConversationState { get; }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public IStatePropertyAccessor<UserData> UserDataAccessor { get; set; }

        public BotAccessors(UserState userState, ConversationState conversationState)
        {
            this.UserState = userState
                ?? throw new ArgumentNullException(nameof(userState));

            this.ConversationState = conversationState
                ?? throw new ArgumentNullException(nameof(conversationState));
        }
    }
}
