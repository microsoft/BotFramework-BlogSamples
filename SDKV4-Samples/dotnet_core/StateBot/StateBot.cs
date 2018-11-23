// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

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
    public class StateBot : IBot
    {
        private readonly StateBotAccessors _accessors;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateBot"/> class.
        /// </summary>
        /// <param name="accessors">A class containing <see cref="IStatePropertyAccessor{T}"/> used to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        /// <remarks>Defines a bot for filling a user profile.</remarks>
        public StateBot(StateBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<StateBot>();
            _logger.LogTrace("EchoBot turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
        }

        /// <summary>The turn handler for the bot.</summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Get the state properties from the turn context.
                UserProfile userProfile = await _accessors.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
                ConversationData conversationData = await _accessors.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

                // First time around this is set to false, so we will prompt user for name
                if (!userProfile.PromptedUserForName)
                {
                    await turnContext.SendActivityAsync($"What is your name?");

                    // Set the flag to true, so we don't prompt in the next turn. 
                    userProfile.PromptedUserForName = true;

                    // Save uderProfile data using the accessors
                    await _accessors.UserProfileAccessor.SetAsync(turnContext, userProfile);
                    await _accessors.UserState.SaveChangesAsync(turnContext);
                }
                else
                {
                    // Set the name to what the user provided
                    userProfile.Name = turnContext.Activity.Text;

                    // Reset the flag to allow the bot to go though the cycle again
                    userProfile.PromptedUserForName = false;

                    // Save the user data
                    await _accessors.UserProfileAccessor.SetAsync(turnContext, userProfile);
                    await _accessors.UserState.SaveChangesAsync(turnContext);

                    // Add message details to the conversation data
                    conversationData.MessageDetails.Add(turnContext.Activity.Timestamp.ToString());
                    conversationData.MessageDetails.Add(turnContext.Activity.ChannelId.ToString());

                    // Update state and save changes.
                    await _accessors.ConversationDataAccessor.SetAsync(turnContext, conversationData);
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);

                    // Display state data
                    await turnContext.SendActivityAsync($"User data: {userProfile.Name}");
                    await turnContext.SendActivityAsync($"Message received at: {turnContext.Activity.Timestamp.ToString()}");
                    await turnContext.SendActivityAsync($"Message received from: {conversationData.MessageDetails[1].ToString()}"); 
                }
            }
        }
    }
}
