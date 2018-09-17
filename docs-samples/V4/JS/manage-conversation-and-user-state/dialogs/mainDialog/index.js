// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Turn counter property
const TURN_COUNTER = 'turnCounter';
const USER_PROFILE = 'userProfile';
const TOPIC_STATE = 'topicState';

class MainDialog {
    /**
     * 
     * @param {Object} conversationState 
     */
    constructor (conversationState, userState) {
        // creates a new state accessor property.see https://aka.ms/about-bot-state-accessors to learn more about the bot state and state accessors 
        this.conversationState = conversationState;
        this.topicState = this.conversationState.createProperty(TOPIC_STATE);

        // User state
        this.userState = userState;
        this.userProfile = this.userState.createProperty(USER_PROFILE);
    }
    /**
     * 
     * @param {Object} context on turn context object.
     */
    async onTurn(context) {
        // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
        if (context.activity.type === 'message') {
            // read from state and set default object if object does not exist in storage.
            let topicState = await this.topicState.get(context, {
                //Define the topic state object
                prompt: "askName"
            });
            let userProfile = await this.userProfile.get(context, {  
                // Define the user's profile object
                "userName": undefined,
                "telephoneNumber": undefined
            });

            if(topicState.prompt == "askName"){
                await context.sendActivity("What is your name?");

                // Set next prompt state
                topicState.prompt = "askNumber";

                // Update state
                await this.topicState.set(context, topicState);
            }
            else if(topicState.prompt == "askNumber"){
                // Set the UserName that is defined in the UserProfile class
                userProfile.userName = context.activity.text;

                // Use the user name to prompt the user for phone number
                await context.sendActivity(`Hello, ${userProfile.userName}. What's your telephone number?`);

                // Set next prompt state
                topicState.prompt = "confirmation";

                // Update states
                await this.topicState.set(context, topicState);
                await this.userProfile.set(context, userProfile);
            }
            else if(topicState.prompt == "confirmation"){
                // Set the phone number
                userProfile.telephoneNumber = context.activity.text;

                // Sent confirmation
                await context.sendActivity(`Got it, ${userProfile.userName}. I'll call you later.`)

                // Set next prompt state
                topicState.prompt = undefined; // We are at the end of our conversation

                // Update states
                await this.topicState.set(context, topicState);
                await this.userProfile.set(context, userProfile);
            }
            
            // Save state changes to storage
            await this.conversationState.saveChanges(context);
            await this.userState.saveChanges(context);
            
        }
        else {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }
    }
}

module.exports = MainDialog;