// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This class is created as a Singleton and passed into the IBot-derived constructor.
    ///  - See <see cref="StateBotAccessors"/> constructor for how that is injected.
    ///  - See the Startup.cs file for more details on creating the Singleton that gets
    ///    injected into the constructor.
    /// </summary>
    public class StateBotAccessors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateBotAccessors"/> class.
        /// Contains the state management and associated accessor objects.
        /// </summary>
        /// <param name="conversationState">The state object that stores the conversation state.</param>
        /// <param name="userState">The state object that stores the user state.</param>
        public StateBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        /// <summary>
        /// Gets the accessor name for the user profile property accessor.
        /// </summary>
        /// <value>The accessor name for the user profile property accessor.</value>
        /// <remarks>Accessors require a unique name.</remarks>
        public static string UserProfileName { get; } = "UserProfile";

        public static string ConversationDataName { get; } = "ConversationData";

        /// <summary>
        /// Gets or sets the <see cref="IStatePropertyAccessor{T}"/> for the user profile property.
        /// </summary>
        /// <value>
        /// The accessor stores the turn count for the conversation.
        /// </value>
        public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; set; }

        public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

        /// <summary>
        /// Gets the <see cref="ConversationState"/> object for the conversation.
        /// </summary>
        /// <value>The <see cref="ConversationState"/> object.</value>
        public ConversationState ConversationState { get; }

        /// <summary>
        /// Gets the <see cref="UserState"/> object for the conversation.
        /// </summary>
        /// <value>The <see cref="UserState"/> object.</value>
        public UserState UserState { get; }
    }
}
