/*
 * Botbuilder v4 SDK - Modular bot logic - wakeUp.js
 * 
 * This file is part of the "modular bot logic" samaple. It contains the logic for a hotel "wake up call" scenario.
 * 
 */

const { ComponentDialog, WaterfallDialog, DateTimePrompt } = require('botbuilder-dialogs');

class WakeUpDialog extends ComponentDialog {
    constructor(dialogId) {
        super(dialogId); 
        this.initialDialogId = "wakeUp"; // Indicate which dialog is the main dialog for this component

        this.dialogs.add(new WaterfallDialog('wakeUp', [
            async function (dc, step) {
                // Create a new local wakeUpInfo databag
                step.values.wakeUpInfo = {};  
                             
                return await dc.prompt('datePrompt', `What time would you like your alarm set for?`);
            },
            async function (dc, step){
                var time = step.result;
                step.values.wakeUpInfo.time = time;
                await dc.context.sendActivity(`Your alarm is set to ${time[0].value}`);
                
                // End the dialog
                return await dc.end(step.values);
            }]));

        // Defining the prompt used in this conversation flow
        this.dialogs.add(new DateTimePrompt('datePrompt'));
    }
}
exports.WakeUp = WakeUpDialog;