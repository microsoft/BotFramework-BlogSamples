// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    public class StateBot : ActivityHandler
    {
        private BotState _conversationState;
        private BotState _userState;
        private StateBotAccessors _accessors;
        private ILogger<StateBot> _logger;

        public StateBot(ConversationState conversationState, UserState userState, ILogger<StateBot> logger)
        {
            _conversationState = conversationState;
            _userState = userState;
            _accessors = new StateBotAccessors(conversationState, userState);
            _logger = logger;
        }

        /// <summary>The turn handler for the bot.</summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Get the state properties from the turn context.

            _accessors.ConversationDataAccessor =  _conversationState.CreateProperty<ConversationData>("StateBotAccessors.ConversationDataName");
            ConversationData conversationData = await _accessors.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());


            _accessors.UserProfileAccessor = _userState.CreateProperty<UserProfile>("StateBotAccessors.UserProfile");
            UserProfile userProfile = await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
           

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // First time around this is set to false, so we will prompt user for name.
                if (conversationData.PromptedUserForName)
                {
                    // Set the name to what the user provided.
                    userProfile.Name = turnContext.Activity.Text?.Trim();

                    // Acknowledge that we got their name.
                    await turnContext.SendActivityAsync($"Thanks {userProfile.Name}.");

                    // Reset the flag to allow the bot to go though the cycle again.
                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    // Prompt the user for their name.
                    await turnContext.SendActivityAsync($"What is your name?");

                    // Set the flag to true, so we don't prompt in the next turn.
                    conversationData.PromptedUserForName = true;
                }

                // Save user state and save changes.
                await _accessors.UserProfileAccessor.SetAsync(turnContext, userProfile);
                await _accessors.UserState.SaveChangesAsync(turnContext);
            }
            else
            {
                // Add message details to the conversation data.
                // Convert saved Timestamp to local DateTimeOffset, then to string for display.
                var messageTimeOffset = (DateTimeOffset) turnContext.Activity.Timestamp;
                var localMessageTime = messageTimeOffset.ToLocalTime();
                conversationData.Timestamp = localMessageTime.ToString();
                conversationData.ChannelId = turnContext.Activity.ChannelId.ToString();

                // Display state data.
                await turnContext.SendActivityAsync($"{userProfile.Name} sent: {turnContext.Activity.Text}");
                await turnContext.SendActivityAsync($"Message received at: {conversationData.Timestamp}");
                await turnContext.SendActivityAsync($"Message received from: {conversationData.ChannelId}");
            }

            // Update conversation state and save changes.
            await _accessors.ConversationDataAccessor.SetAsync(turnContext, conversationData);
            await _accessors.ConversationState.SaveChangesAsync(turnContext);
        }
    }
}

