// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { ActivityTypes } = require('botbuilder');
const { DialogSet, WaterfallDialog, NumberPrompt, DateTimePrompt, ChoicePrompt, DialogTurnStatus } = require('botbuilder-dialogs');

// Define identifiers for our state property accessors.
const DIALOG_STATE_ACCESSOR = 'dialogStateAccessor';
const RESERVATION_ACCESSOR = 'reservationAccessor';

// Define identifiers for our dialogs and prompts.
const RESERVATION_DIALOG = 'reservationDialog';
const PARTY_SIZE_PROMPT = 'partySizePrompt';
const LOCATION_PROMPT = 'locationPrompt';
const RESERVATION_DATE_PROMPT = 'reservationDatePrompt';

class DialogPromptBot {
    /**
     *
     * @param {ConversationState} conversation state object
     */
    constructor(conversationState) {
        // Creates our state accessor properties.
        // See https://aka.ms/about-bot-state-accessors to learn more about the bot state and state accessors.
        this.dialogStateAccessor = conversationState.createProperty(DIALOG_STATE_ACCESSOR);
        this.reservationAccessor = conversationState.createProperty(RESERVATION_ACCESSOR);
        this.conversationState = conversationState;

        // Create the dialog set and add the prompts, including custom validation.
        this.dialogSet = new DialogSet(this.dialogStateAccessor);
        this.dialogSet.add(new NumberPrompt(PARTY_SIZE_PROMPT, this.partySizeValidator));
        this.dialogSet.add(new ChoicePrompt (LOCATION_PROMPT));
        this.dialogSet.add(new DateTimePrompt(RESERVATION_DATE_PROMPT, this.dateValidator));

        // Define the steps of the waterfall dialog and add it to the set.
        this.dialogSet.add(new WaterfallDialog(RESERVATION_DIALOG, [
            this.promptForPartySize.bind(this),
            this.promptForLocation.bind(this),
            this.promptForReservationDate.bind(this),
            this.acknowledgeReservation.bind(this),
        ]));
    }

    async partySizeValidator(promptContext) {
        // Check whether the input could be recognized as an integer.
        if (!promptContext.recognized.succeeded) {
            await promptContext.context.sendActivity(
                "I'm sorry, I do not understand. Please enter the number of people in your party.");
            return false;
        }
        if (promptContext.recognized.value % 1 != 0) {
            await promptContext.context.sendActivity(
                "I'm sorry, I don't understand fractional people.");
            return false;
        }
        // Check whether the party size is appropriate.
        var size = promptContext.recognized.value;
        if (size < 6 || size > 20) {
            await promptContext.context.sendActivity(
                'Sorry, we can only take reservations for parties of 6 to 20.');
            return false;
        }

        return true;
    }

    async dateValidator(promptContext) {
        // Check whether the input could be recognized as an integer.
        if (!promptContext.recognized.succeeded) {
            await promptContext.context.sendActivity(
                "I'm sorry, I do not understand. Please enter the date or time for your reservation.");
            return false;
        }

        // Check whether any of the recognized date-times are appropriate,
        // and if so, return the first appropriate date-time.
        const earliest = Date.now() + (60 * 60 * 1000);
        let value = null;
        promptContext.recognized.value.forEach(candidate => {
            // TODO: update validation to account for time vs date vs date-time vs range.
            const time = new Date(candidate.value || candidate.start);
            if (earliest < time.getTime()) {
                value = candidate;
            }
        });
        if (value) {
            promptContext.recognized.value = [value];
            return true;
        }

        await promptContext.context.sendActivity(
            "I'm sorry, we can't take reservations earlier than an hour from now.");
        return false;
    }

    async promptForPartySize(stepContext) {
        // Prompt for the party size. The result of the prompt is returned to the next step of the waterfall.
        return await stepContext.prompt(
            PARTY_SIZE_PROMPT, {
                prompt: 'How many people is the reservation for?',
                retryPrompt: 'How large is your party?'
            });
    }

    async promptForLocation(stepContext) {
        // Prompt for location
        return await stepContext.prompt(
            LOCATION_PROMPT, 'Select a location.', ['Redmond', 'Bellevue', 'Seattle']
        );
    }

    async promptForReservationDate(stepContext) {
        // Record the party size information in the current dialog state.
        stepContext.values.size = stepContext.result;

        // Prompt for the party size. The result of the prompt is returned to the next step of the waterfall.
        return await stepContext.prompt(
            RESERVATION_DATE_PROMPT, {
                prompt: 'Great. When will the reservation be for?',
                retryPrompt: 'What time should we make your reservation for?'
            });
    }

    async acknowledgeReservation(stepContext) {
        // Retrieve the reservation date.
        const resolution = stepContext.result[0];
        const time = resolution.value || resolution.start;

        // Send an acknowledgement to the user.
        await stepContext.context.sendActivity(
            'Thank you. We will confirm your reservation shortly.');

        // Return the collected information to the parent context.
        return await stepContext.endDialog({ date: time, size: stepContext.values.size });
    }

    /**
     *
     * @param {TurnContext} on turn context object.
     */
    async onTurn(turnContext) {
        switch (turnContext.activity.type) {
            case ActivityTypes.Message:
                // Get the current reservation info from state.
                const reservation = await this.reservationAccessor.get(turnContext, null);

                // Generate a dialog context for our dialog set.
                const dc = await this.dialogSet.createContext(turnContext);

                if (!dc.activeDialog) {
                    // If there is no active dialog, check whether we have a reservation yet.
                    if (!reservation) {
                        // If not, start the dialog.
                        await dc.beginDialog(RESERVATION_DIALOG);
                    }
                    else {
                        // Otherwise, send a status message.
                        await turnContext.sendActivity(
                            `We'll see you ${reservation.date}.`);
                    }
                }
                else {
                    // Continue the dialog.
                    const dialogTurnResult = await dc.continueDialog();

                    // If the dialog completed this turn, record the reservation info.
                    if (dialogTurnResult.status === DialogTurnStatus.complete) {
                        await this.reservationAccessor.set(
                            turnContext,
                            dialogTurnResult.result);

                        // Send a confirmation message to the user.
                        await turnContext.sendActivity(
                            `Your party of ${dialogTurnResult.result.size} is ` +
                            `confirmed for ${dialogTurnResult.result.date}.`);
                    }
                }

                // Save the updated dialog state into the conversation state.
                await this.conversationState.saveChanges(turnContext, false);
                break;
            case ActivityTypes.EndOfConversation:
            case ActivityTypes.DeleteUserData:
                break;
            default:
                break;
        }
    }
}

module.exports.DialogPromptBot = DialogPromptBot;