// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityTypes } = require('botbuilder');

// The accessor names for the conversation data and user profile state property accessors.
const CONVERSATION_DATA_PROPERTY = 'conversationData';
const USER_PROFILE_PROPERTY = 'userProfile';

class MyBot {
    /**
     *
     * @param {ConversationState} conversation state object
     */
    constructor(conversationState, userState) {
        // Create the state property accessors for the conversation data and user profile.
        this.conversationData = conversationState.createProperty(CONVERSATION_DATA_PROPERTY);
        this.userProfile = userState.createProperty(USER_PROFILE_PROPERTY);

        // The state management objects for the conversation and user state.
        this.conversationState = conversationState;
        this.userState = userState;
    }

    /**
     *
     * @param {TurnContext} on turn context object.
     */
    async onTurn(turnContext) {
        // See https://aka.ms/about-bot-activity-message to learn more about the message and other activity types.
        if (turnContext.activity.type === ActivityTypes.Message) {
            // Get the state properties from the turn context.
            const userProfile = await this.userProfile.get(turnContext, {});
            const conversationData = await this.conversationData.get(
                turnContext, { promptedForUserName: false });

            if (!userProfile.name) {
                // First time around this is undefined, so we will prompt user for name.
                if (conversationData.promptedForUserName) {
                    // Set the name to what the user provided.
                    userProfile.name = turnContext.activity.text;

                    // Acknowledge that we got their name.
                    await turnContext.sendActivity(`Thanks ${userProfile.name}.`);

                    // Reset the flag to allow the bot to go though the cycle again.
                    conversationData.promptedForUserName = false;
                } else {
                    // Prompt the user for their name.
                    await turnContext.sendActivity('What is your name?');

                    // Set the flag to true, so we don't prompt in the next turn.
                    conversationData.promptedForUserName = true;
                }
                // Save user state and save changes.
                await this.userProfile.set(turnContext, userProfile);
                await this.userState.saveChanges(turnContext);
            } else {
                // Add message details to the conversation data.
                conversationData.timestamp = turnContext.activity.timestamp.toLocaleString();
                conversationData.channelId = turnContext.activity.channelId;

                // Display state data.
                await turnContext.sendActivity(`${userProfile.name} sent: ${turnContext.activity.text}`);
                await turnContext.sendActivity(`Message received at: ${conversationData.timestamp}`);
                await turnContext.sendActivity(`Message received from: ${conversationData.channelId}`);
            }
            // Update conversation state and save changes.
            await this.conversationData.set(turnContext, conversationData);
            await this.conversationState.saveChanges(turnContext);
        }
    }
}

module.exports.MyBot = MyBot;