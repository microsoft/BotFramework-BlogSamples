const { BotFrameworkAdapter, BotStateSet, FileStorage, ConversationState, UserState } = require('botbuilder');
const { DialogSet, TextPrompt, ChoicePrompt, DatetimePrompt, NumberPrompt, ConfirmPrompt } = require("botbuilder-dialogs");
const restify = require('restify');
const { QnAMaker } = require('botbuilder-ai');
require('dotenv').config()

// Create server
let server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`${server.name} listening to ${server.url}`);
});

// Create adapter
const adapter = new BotFrameworkAdapter({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

// Add QnA Maker
const qnaMaker = new QnAMaker(
    {
        knowledgeBaseId: process.env.QNA_KNOWLEDGE_BASE_ID,
        endpointKey: process.env.QNA_ENDPOINT_KEY,
        host: process.env.QNA_HOST,
    },
    {
        scoreThreshold: process.env.QNA_SCORE_THRESHOLD
    }
);

// Add state middleware
const storage = new FileStorage("C:/temp");
const convoState = new ConversationState(storage);
const userState = new UserState(storage);
adapter.use(new BotStateSet(convoState, userState));

// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        if (context.activity.type !== 'message') {
            // Handle any non-message activity.
            switch (context.activity.type) {
                // Not all channels send a ConversationUpdate activity.
                // However, both the Emulator and WebChat do.
                case "conversationUpdate":
                    // If a user is being added to the conversation, send them an initial greeting.
                    if (context.activity.membersAdded[0].name !== 'Bot') {
                        await context.sendActivity("Hello, I'm the Contoso Cafe bot.")
                        await context.sendActivity(`How can I help you? (Type "book a table" to set up a table reservation.)`)
                    }
            }
        } else {
            // Get conversation state and establish a dialog context.
            const convo = convoState.get(context);
            const dc = dialogs.createContext(context, convo);

            // Capture any input text.
            const text = (context.activity.text || '').trim().toLowerCase();

            if (text === "cancel" || text === "start over" || text === "stop") {
                // If there's no active dialog, this is a no-op.
                dc.endAll();

                // Send a cancellation message and finish turn.
                await context.sendActivity("Sure.. Let's start over");
                return;
            }

            // Continue the current dialog if one is currently active
            await dc.continue();

            // If there were an active dialog, then it should have replied to the user.
            if (!context.responded) {
                // Handle any "command-like" input from the user.
                switch (text) {
                    case "book table":
                    case "book a table":
                        // Stub for booking a table.
                        await dc.begin("bookATable");
                        break;

                    case "help":
                        // Provide some guidance to the user.
                        await context.sendActivity(`Type "book a table" to make a reservation.`);
                        break;
                }
            }

            if (!context.responded){
                // Field any questions the user has asked.
                var answers = await qnaMaker.generateAnswer(text);

                if(answers == null) {
                    await context.sendActivity("Call to the QnA Maker service failed.")
                }
                else if (answers && answers.length > 0) {
                    // If the service produced one or more answers, send the first one.
                    await context.sendActivity(answers[0].answer);
                } 
            }

            if(!context.responded){
                // Provide a default response for anything we don't understand.
                await context.sendActivity("I'm sorry; I do not understand.");
                await context.sendActivity(`Type "book a table" to make a reservation.`);
            }
        }
    });

});

const dialogs = new DialogSet();

dialogs.add('bookATable', [
    async function (dc, args) {

        // Begin booking a table

        // Query for location
        const locations = ["Bellevue", "Redmond", "Renton", "Seattle"];
        await dc.prompt('choicePrompt', 'Please select one of our locations.', locations, { retryPrompt: 'Please select only these locations.' });
    },
    async function (dc, result) {
        //  Update state with the location 
        dc.activeDialog.state.location = result.value;

        await dc.prompt('dateTimePrompt', "When will the reservation be for?", { retryPrompt: 'Please enter a date and time for the reservation.' });
    },
    async function (dc, result) {
        //  Update state with the date and time
        dc.activeDialog.state.dateTime = result[0].value;

        // Ask for the reservation name next
        await dc.prompt('numberPrompt', "How many guests?", { retryPrompt: "Please enter the number of people that the reservation is for." });
    },
    async function (dc, result) {
        // Update state with the number of guests
        dc.activeDialog.state.guests = result;

        // Query for a name for the resevation.
        await dc.prompt('textPrompt', "What name should I book the table under?", { retryPrompt: "Please enter a name for the reservation." })
    },
    async function (dc, result) {
        // Update state with the name for the reservation.
        dc.activeDialog.state.name = result;

        await dc.prompt('confirmPrompt', `Ok. Should I go ahead and book a table
        for ${dc.activeDialog.state.guests} 
        at ${dc.activeDialog.state.location}
        for ${dc.activeDialog.state.dateTime}
        for ${dc.activeDialog.state.name}?`, {
                retryPrompt: `I'm sorry, should I make the reservation for you?
        Please enter "yes" or "no".`})
    },
    async function (dc, result) {
        var confirmed = result;

        if (confirmed) {
            // Send a confirmation message
            await dc.context.sendActivities([
                { type: 'typing' },
                { type: 'delay', value: 2000 },
                { type: 'message', text: 'Your table is booked. Reference number: #K89HG38SZ' }
            ]);
            await dc.end();
        } else {
            // Decide what to do if they say no at this point.
            await dc.context.sendActivity("Okay. We have canceled the reservation.")
            await dc.end();
        }
    }
]);


dialogs.add('textPrompt', new TextPrompt)
dialogs.add('choicePrompt', new ChoicePrompt)
// dialogs.add('dateTimePrompt', new DatetimePrompt)
dialogs.add('dateTimePrompt', new DatetimePrompt(
    async (context, values) => {
        let values_to_return = [];
        for (let value of values) {

            var errmsg = "";
            const resolution = new Date(value.value);
            const now = new Date().getTime();
            // Get one day in milliseconds
            const one_day = 1000 * 60 * 60 * 24;
            const two_weeks = one_day * 14;
            if (resolution.getTime() < new Date().getTime()) {
                errmsg = "time is in the past.";
            } else if (resolution.getTime() > now + two_weeks) {
                errmsg = "time is more than 2 weeks from now";
            } if (resolution.getHours() < 16) {
                errmsg = "Time was before 4pm.";
            } else if (resolution.getHours() > 20 ) {
                errmsg = "Time after 8pm."
            }
            if (!errmsg) {
                values_to_return.push(value);
            } else {
                console.log("Didn't push value: " + errmsg);
            }
        }
        if (values_to_return.length == 0) {
            context.sendActivity(errmsg);
        }
        return values_to_return;
    }
));
dialogs.add('confirmPrompt', new ConfirmPrompt)
dialogs.add('numberPrompt', new NumberPrompt(async (context, value) => {
    try {
        if (isNaN(value)) {
            throw new Error('Party size too small.')
        }
        if (value < 1) {
            throw new Error('Party size too small.');
        }
        else if (value > 13) {
            throw new Error('Party size too big.')
        }
        else {
            return value; // Return the valid value
        }
    } catch (err) {
        await context.sendActivity(`${err.message} <br/>Please provide a party size between 1 and 12.`);
        return undefined;
    }
}));
