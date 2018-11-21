// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This class is created as a Singleton and passed into the IBot-derived constructor.
    ///  - See <see cref="DialogPromptBot"/> constructor for how that is injected.
    ///  - See the Startup.cs file for more details on creating the Singleton that gets
    ///    injected into the constructor.
    /// </summary>
    public class DialogPromptBotAccessors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogPromptBotAccessors"/> class.
        /// Contains the <see cref="ConversationState"/> and associated <see cref="IStatePropertyAccessor{T}"/>.
        /// </summary>
        /// <param name="conversationState">The state object that stores the counter.</param>
        public DialogPromptBotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public static string DialogStateAccessorKey { get; } = "DialogPromptBotAccessors.DialogState";

        public static string ReservationAccessorKey { get; } = "DialogPromptBotAccessors.Reservation";

        /// <summary>
        /// Gets or sets the <see cref="IStatePropertyAccessor{T}"/> for CounterState.
        /// </summary>
        /// <value>
        /// The accessor stores the turn count for the conversation.
        /// </value>
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public IStatePropertyAccessor<DialogPromptBot.Reservation> ReservationAccessor { get; set; }

        /// <summary>
        /// Gets the <see cref="ConversationState"/> object for the conversation.
        /// </summary>
        /// <value>The <see cref="ConversationState"/> object.</value>
        public ConversationState ConversationState { get; }
    }
}
