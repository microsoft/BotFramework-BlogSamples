// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityTypes } = require('botbuilder');
const { DialogSet, WaterfallDialog, TextPrompt, NumberPrompt } = require('botbuilder-dialogs');

class MainDialog {
    /**
     * 
     * @param {Object} conversationState 
     * @param {Object} userState 
     */
    constructor (conversationState, userState) {

        // creates a new state accessor property. see https://aka.ms/about-bot-state-accessors to learn more about the bot state and state accessors 
        this.conversationState = conversationState;
        this.userState = userState;

        this.dialogState = this.conversationState.createProperty('dialogState');

        this.userInfoAccessor = this.userState.createProperty('userInfo');

        this.dialogs = new DialogSet(this.dialogState);
        
        // Add prompts
        this.dialogs.add(new TextPrompt('textPrompt'));
        this.dialogs.add(new NumberPrompt('numberPrompt'));
        
        // Check in user:
        this.dialogs.add(new WaterfallDialog('checkIn', [
            async function (step) {
                // Create a new local guestInfo databag
                step.values.guestInfo = {};
                return await step.prompt('textPrompt', "Welcome to the 'Check In' service. <br/>What is your name?");
            },
            async function (step){
                // Save the name 
                var name = step.result;
                step.values.guestInfo.name = name;
                return await step.prompt('numberPrompt', `Hi ${name}. What room will you be staying in?`);
            },
            async function (step){
                // Save the room number
                var room = step.result;
                step.values.guestInfo.room = room
                await step.context.sendActivity(`Great, room ${room} is ready for you. Enjoy your stay!`);

                // End the dialog and return the guest info
                return await step.endDialog(step.values);
            }
        ]));


    }

    /**
     * 
     * @param {Object} turnContext on turn context object.
     */
    async onTurn(turnContext) {
        const isMessage = (turnContext.activity.type === 'message');
        // See https://aka.ms/about-bot-activity-message to learn more about the message and other activity types.
        if (isMessage) {
            // Create dialog context
            const dc = await this.dialogs.createContext(turnContext);

            // Handle continued conversation
            if(!turnContext.responded){
                // Continue executing the "current" dialog, if any.
                var results = await dc.continueDialog();

                // The dialog is complete with data passed back.
                if(results.status == "complete" && results.result){
                    // Do something with `results.result`
                    const userInfo = await this.userInfoAccessor.get(turnContext, {});

                    // Persist data in appropriate bags
                    if(results.result.guestInfo){
                        userInfo.guestInfo = results.result.guestInfo;
                    }
                }

                if(!turnContext.responded && isMessage){
                    // Default message
                    await turnContext.sendActivity("Hi! I'm a simple bot. Please say 'check in'.");
                }
            }
            
            // Check for valid intents
            if(turnContext.activity.text.match(/check in/ig)){
                await dc.beginDialog('checkIn');
            }

        } else if (
            turnContext.activity.type === ActivityTypes.ConversationUpdate &&
            turnContext.activity.membersAdded[0].name !== 'Bot'
        ) {
           // send a "this is what the bot does" message
            await turnContext.sendActivity('I am a bot that demonstrates a simple conversation. Say anything to continue.');
        }

        // Save changes to the user name.
        await this.userState.saveChanges(turnContext);

        // End this turn by saving changes to the conversation state.
        await this.conversationState.saveChanges(turnContext);

    }

}

module.exports = MainDialog;