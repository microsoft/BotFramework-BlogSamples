const { BotFrameworkAdapter, BotStateSet, ConversationState, UserState, MemoryStorage } = require('botbuilder');
const { LuisRecognizer } = require('botbuilder-ai');
const { DialogSet, TextPrompt, ChoicePrompt, DatetimePrompt, NumberPrompt, ConfirmPrompt } = require("botbuilder-dialogs");
const restify = require('restify');
const { QnAMaker } = require('botbuilder-ai');
require('dotenv').config()

// We use these classes to validate datetime entities detected by LUIS
const Resolver = require('@microsoft/recognizers-text-data-types-timex-expression').default.resolver;
const Creator = require('@microsoft/recognizers-text-data-types-timex-expression').default.creator;
const TimexProperty = require('@microsoft/recognizers-text-data-types-timex-expression').default.TimexProperty;

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

// Set up a LUIS recognizer
// LUIS_APP_ID LUIS_SUBSCRIPTION_KEY in the .env file
const appId = process.env.LUIS_APP_ID;
const subscriptionKey = process.env.LUIS_SUBSCRIPTION_KEY;
// Default is westus
const serviceEndpoint = 'https://westus.api.cognitive.microsoft.com';

const luisRec = new LuisRecognizer({
    appId: appId,
    subscriptionKey: subscriptionKey,
    serviceEndpoint: serviceEndpoint
});

// Add state middleware
const storage = new MemoryStorage();
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
                        await context.sendActivity("How can I help you? (Type `book a table` to set up a table reservation.)")
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
                await luisRec.recognize(context).then(async (res) => {
                    let topIntent = LuisRecognizer.topIntent(res);
                    switch (topIntent) {
                        case "Book_Table": {
                            await context.sendActivity("Top intent is Book_Table ");
                            await dc.begin('bookATable', res);
                            break;
                        }

                        case "Greeting": {
                            await context.sendActivity("Hello.");
                            break;
                        }

                        // "None" and any other intents fall through to QnA
                        case "None":
                        default: {
                            await context.sendActivity(`Reached default case. Top intent is ${topIntent}`);
                            // Field any questions the user has asked.
                            var answers = await qnaMaker.generateAnswer(text);

                            if (answers == null) {
                                await context.sendActivity("Call to the QnA Maker service failed.")
                            }
                            else if (answers && answers.length > 0) {
                                // If the service produced one or more answers, send the first one.
                                await context.sendActivity(answers[0].answer);
                            }
                            break;
                        }
                    }
                }, (err) => {
                    console.log(`Error calling recognize() ${err}`);
                });
            }

            if (!context.responded) {
                // Provide a default response if the bot hasn't responded yet.
                // This could happen if QnA couldn't find a response in the QnA fallback case
                await context.sendActivity("I'm sorry; I do not understand.");
                await context.sendActivity("Type `book a table` to make a reservation.");
            }
        }
    });
});

// Create an empty dialog set
const dialogs = new DialogSet();

// Add a bookATable dialog to the set of dialogs
dialogs.add('bookATable', [
    async function (dc, args, next) {

        var luisresult = args;
        // Call a helper function to save the entities in the LUIS result
        // to dialog state
        await SaveEntities(dc, luisresult);

        // Begin booking a table
        if (dc.activeDialog.state.cafeLocation) {
            await next();
        } else {
            // Query for location
            const locations = ["Bellevue", "Redmond", "Renton", "Seattle"];
            await dc.prompt('choicePrompt', 'Please select one of our locations.', locations, { retryPrompt: 'Please select only these locations.' });
        }
    },
    async function (dc, result, next) {
        // If we don't already have location saved from the LUIS entities,
        // update state with the location in the prompt result
        if (!dc.activeDialog.state.cafeLocation) {
            dc.activeDialog.state.cafeLocation = result.value;
        }

        if (dc.activeDialog.state.dateTime) {
            await next();
        } else {
            await dc.prompt('dateTimePrompt', "When will the reservation be for?", { retryPrompt: 'Please enter a date and time for the reservation.' });
        }
    },
    async function (dc, result, next) {
        if (!dc.activeDialog.state.dateTime) {
            //  Update state with the date and time.
            // The prompt validator had some logic to return only dates in a valid time and date range.
            // Since the date has already been validated, we just take the first one 
            // from the list of date resolutions that the dateTime promt returns.
            dc.activeDialog.state.dateTime = result[0].value;
        }

        // If we don't have party size, ask for it next
        if (!dc.activeDialog.state.partySize) {
            // Ask for the number of guests next        
            await dc.prompt('numberPrompt', "How many guests?", { retryPrompt: "Please enter the number of people that the reservation is for." });
        } else {
            await next();
        }
    },
    async function (dc, result, next) {
        // Update state with the number of guests
        if (!dc.activeDialog.state.partySize) {
            dc.activeDialog.state.partySize = result;
        }

        // Query for a name for the resevation.
        await dc.prompt('textPrompt', "What name should I book the table under?", { retryPrompt: "Please enter a name for the reservation." })
    },
    async function (dc, result) {
        // Update state with the name for the reservation.
        dc.activeDialog.state.name = result;

        // TODO: rename partySize to guests
        await dc.prompt('confirmPrompt', `Ok. Should I go ahead and book a table
        for ${dc.activeDialog.state.partySize}
        at ${dc.activeDialog.state.cafeLocation}
        for ${dc.activeDialog.state.dateTime}
        for ${dc.activeDialog.state.name}?`, {
                retryPrompt: `I'm sorry, should I make the reservation for you?
        Please enter "yes" or "no".`})
    },
    async function (dc, result) {
        var confirmed = result;

        if (confirmed) {
            // Copy the dialog state to the conversation state
            var state = convoState.get(dc.context);
            state = dc.activeDialog.state;

            // Send a confirmation message
            await dc.context.sendActivities([
                { type: 'typing' },
                { type: 'delay', value: 2000 },
                {
                    type: 'message', text: `Your table is booked. Reservation details:             
                Date/Time: ${state.dateTime} 
                Party size: ${state.partySize} 
                Reservation name: ${state.name}`
                }
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
            throw new Error('Party size invalid.')
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

// Helper function that saves any entities found in the LUIS result
// to the dialog state
async function SaveEntities(dc, luisresult) {
    // Resolve entities returned from LUIS, and save these to state
    if (luisresult.entities) {

        let datetime = luisresult.entities.datetime;

        if (datetime) {
            console.log(`datetime entity found of type ${datetime[0].type}.`);

            // Use the first date or time found in the utterance            
            if (datetime[0].timex) {
                var timexValues = datetime[0].timex
                // timexValues is the array of all resolutions of datetime[0]
                // a datetime entity detected by LUIS is resolved to timex format.
                // More information on timex can be found here: 
                // http://www.timeml.org/publications/timeMLdocs/timeml_1.2.1.html#timex3                                
                // More information on the library which does the recognition can be found here: 
                // https://github.com/Microsoft/Recognizers-Text
                if (datetime[0].type === "datetime") {
                    var resolution = Resolver.evaluate(
                        // array of timex values to evaluate. There may be more than one: "today at 6" can be 6AM or 6PM.
                        timexValues,
                        // Creator.evening constrains this to times between 4pm and 8pm
                        [Creator.evening, Creator.nextWeeksFromToday(2)]);
                    if (resolution[0]) {
                        // toNaturalLanguage takes the current date into account to create a friendly string
                        dc.activeDialog.state.dateTime = resolution[0].toNaturalLanguage(new Date());
                        // You can also use resolution.toString() to format the date/time.
                    } else {
                        // time didn't satisfy constraint.
                        dc.activeDialog.state.dateTime = null;
                    }
                }
                else {
                    console.log(`Type ${datetime[0].type} is not yet supported. Provide both the date and the time.`);
                }
            }
        }
        let partysize = luisresult.entities.partySize;
        if (partysize) {
            console.log(`partysize entity detected: ${partysize}`);
            // use first partySize entity that was found in utterance
            dc.activeDialog.state.partySize = partysize[0];
        }
        let cafelocation = luisresult.entities.cafeLocation;

        if (cafelocation) {
            console.log(`location entity detected: ${cafelocation}`);
            // use first cafeLocation entity that was found in utterance
            dc.activeDialog.state.cafeLocation = cafelocation[0][0];
        }
    }
}