/*
 * Botbuilder v4 SDK - Modular bot logic - checkIn.js
 * 
 * This file is part of the "modular bot logic" samaple. It contains the logic for a hotel check in scenario.
 * 
 */
const { MessageFactory } = require('botbuilder');
const { Dialog, ComponentDialog, WaterfallDialog, TextPrompt, NumberPrompt } = require('botbuilder-dialogs');

class CheckInDialog extends ComponentDialog {
    constructor(dialogId) {
        super(dialogId);
        this.initialDialogId = "checkIn"; // Indicate which dialog is the main dialog for this component

        // Defining the conversation flow using a waterfall model
        this.dialogs.add(new WaterfallDialog('checkIn', [
            async function (dc, step) {
                // Create a new local guestInfo databag
                step.values.guestInfo = {};
                //return await dc.prompt('textPrompt', "Welcome to the 'Check In' service. <br/>What is your name?");
                
                await dc.context.sendActivity("Welcome to the 'Check In' service. <br/>What is your name?");
                return Dialog.EndOfTurn;

                // This will error on next turn due to dialog being removed from stack
                //return await dc.context.sendActivity("Welcome to the 'Check In' service. <br/>What is your name?"); 
                
            },
            async function (dc, step){
                // Save the name 
                var name = step.result;
                step.values.guestInfo.name = name;
                return await dc.prompt('numberPrompt', `Hi ${name}. What room will you be staying in?`);
            },
            async function (dc, step){
                // Save the room number
                var room = step.result;
                step.values.guestInfo.room = room
                await dc.context.sendActivity(`Great, room ${room} is ready for you. Enjoy your stay!`);

                // End the dialog and return the guest info
                return await dc.end(step.values);
            }
        ]));
        // Defining the prompt used in this conversation flow
        this.dialogs.add(new TextPrompt('textPrompt'));
        this.dialogs.add(new NumberPrompt('numberPrompt'));
    }
}
exports.CheckIn = CheckInDialog;