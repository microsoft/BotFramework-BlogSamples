/*
 * Botbuilder v4 SDK - Modular bot logic - reserveTable.js
 * 
 * This file is part of the "modular bot logic" samaple. It contains the logic for a hotel "reserve table" scenario.
 * 
 */

const { ComponentDialog, WaterfallDialog, ChoicePrompt } = require('botbuilder-dialogs');

class ReserveTableDialog extends ComponentDialog {
    constructor(dialogId) {
        super(dialogId); 
        this.initialDialogId = "reserveTable"; // Indicate which dialog is the main dialog for this component

        // Defining the conversation flow using a waterfall model
        this.dialogs.add(new WaterfallDialog('reserveTable', [
            async function (dc, step) {
                // Create a new local tableInfo databag
                step.values.tableInfo = {};
        
                const prompt = `Which table would you like to reserve?`;
                const choices = ['1', '2', '3', '4', '5', '6'];
                return await dc.prompt('choicePrompt', prompt, choices);
            },
            async function (dc, step) {
                // Create a new local tableInfo databag
                step.values.tableInfo.tableNumber = step.result.value;
        
                return await dc.prompt('textPrompt', `What is the reservation name?`);
            },
            async function(dc, step){
                step.values.tableInfo.reserveName = step.result;
                await dc.context.sendActivity(`Got it! Table number ${step.values.tableInfo.tableNumber} is reserved for ${step.values.tableInfo.reserveName}.`);
                
                // End the dialog and return the table information
               return  await dc.end(step.values);
            }
        ]));

        // Defining the prompt used in this conversation flow
        this.dialogs.add(new ChoicePrompt('choicePrompt'));
    }
}
exports.ReserveTable = ReserveTableDialog;