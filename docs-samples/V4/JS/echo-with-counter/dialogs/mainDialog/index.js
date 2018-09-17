// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Turn counter property
const TURN_COUNTER = 'turnCounter';

class MainDialog {
    /**
     * 
     * @param {Object} conversationState 
     */
    constructor (conversationState) {
        // creates a new state accessor property.see https://aka.ms/about-bot-state-accessors to learn more about the bot state and state accessors 
        this.conversationState = conversationState;
        this.countProperty = this.conversationState.createProperty(TURN_COUNTER);
    }
    /**
     * 
     * @param {Object} context on turn context object.
     */
    async onTurn(context) {
        // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
        if (context.activity.type === 'message') {
            // read from state.
            let count = await this.countProperty.get(context);
            count = count === undefined ? 1 : ++count;
            await context.sendActivity(`${count}: You said "${context.activity.text}"`);
            await this.countProperty.set(context, count);
            await this.conversationState.saveChanges(context);
        }
        else {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }
    }
}

module.exports = MainDialog;