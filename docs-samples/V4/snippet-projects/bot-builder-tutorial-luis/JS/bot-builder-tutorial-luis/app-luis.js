const { BotFrameworkAdapter, MemoryStorage, ConversationState } = require('botbuilder');
const { LuisRecognizer } = require('botbuilder-ai');
const { DialogSet, DatetimePrompt, TextPrompt } = require('botbuilder-dialogs')
const Resolver = require('@microsoft/recognizers-text-data-types-timex-expression').default.resolver;
const Creator = require('@microsoft/recognizers-text-data-types-timex-expression').default.creator;
const TimexProperty = require('@microsoft/recognizers-text-data-types-timex-expression').default.TimexProperty;
const restify = require('restify');
require('dotenv').config()

// LUIS_APP_ID in the .env file
const appId = process.env.LUIS_APP_ID;
// LUIS_SUBSCRIPTION_KEY in the .env file
const subscriptionKey = process.env.LUIS_SUBSCRIPTION_KEY;

// Default is westus
const serviceEndpoint = 'https://westus.api.cognitive.microsoft.com';

const luisRec = new LuisRecognizer({
    appId: appId,
    subscriptionKey: subscriptionKey,
    serviceEndpoint: serviceEndpoint
});

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

// Add conversation state middleware
const conversationState = new ConversationState(new MemoryStorage());
adapter.use(conversationState);

// Create empty dialog set
const dialogs = new DialogSet();

// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        const isMessage = context.activity.type === 'message';

        // set up conversation state
        const state = conversationState.get(context);

        // Create dialog context based on state
        const dc = dialogs.createContext(context, state);

        if (!isMessage) {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }

        // Check to see if anyone replied. 
        if (!context.responded) {
            await dc.continue();
            // if the dialog didn't send a response
            if (!context.responded && isMessage) {
                await luisRec.recognize(context).then(async (res) => {
                    let topIntent = LuisRecognizer.topIntent(res);
                    switch (topIntent) {
                        case "Book_Table": {
                            await context.sendActivity("Top intent is Book_Table ");
                            await dc.begin('reserveTable', res);
                            break;
                        }

                        case "Greeting": {
                            await context.sendActivity("Top intent is Greeting");
                            break;
                        }

                        case "Who_are_you_intent": {
                            await context.sendActivity("Top intent is Who_are_you_intent");
                            break;
                        }
                        default: {
                            await context.sendActivity(`Top intent is ${topIntent}`);
                            // await dc.begin('default', topIntent);
                            break;
                        }
                    }
                }, (err) => {
                    // error in recognize()
                    console.log(err);
                });
            }
        }

    });
});

// Add dialogs
dialogs.add('default', [
    async function (dc, args) {
        const state = conversationState.get(dc.context);
        await dc.context.sendActivity(`Hi! I'm the Contoso Cafe reservation bot. Say something like make a reservation."`);
        await dc.end();
    }
]);


dialogs.add('textPrompt', new TextPrompt());

dialogs.add('dateTimePrompt', new DatetimePrompt(
    async (context, values) => {
        try {
            if (values.length <= 0) {
                console.log(`Length of values array in prompt validator was 0`);
                throw new Error('Length of values array in prompt validator <= 0');
            }

            if (values[0].type !== 'datetime') {
                console.log(`unsupported type ${values[0].type}. expected: datetime.`);
                throw new Error(`unsupported type ${values[0].type}. expected: datetime.`);
            }

            var TimexBugFixed = false;
            if (TimexBugFixed) {
                var resolution = Resolver.evaluate(
                    // array of timex values to evaluate. 
                    // There may be more than one: "today at 6" can be 6AM or 6PM.
                    values,
                    // Creator.evening constrains this to times between 4pm and 8pm
                    [Creator.evening, Creator.nextWeeksFromToday(2)]);

                return resolution;
            } else {
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
                    } else if (resolution.getHours() > 20) {
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

        } catch (err) {
            console.log(`${err.name, err.message, err.stack}`)
            //await context.sendActivity(`${err.message}`);
            return undefined;
        }
    }
));

dialogs.add('reserveTable', [
    async function (dc, args, next) {
        var luisresult = args;

        // Call a helper function to save the entities in the LUIS result
        // to dialog state
        await SaveEntities(dc, luisresult);

        await dc.context.sendActivity("Welcome to the reservation service.");

        if (dc.activeDialog.state.dateTime) {
            await next();
        }
        else {
            await dc.prompt('dateTimePrompt', "Please provide a reservation date and time. We're open 4PM-8PM.");
        }
    },
    async function (dc, result, next) {
        if (!dc.activeDialog.state.dateTime) {
            // Save the dateTimePrompt result to dialog state
            dc.activeDialog.state.dateTime = result[0].value;
        }

        // If we don't have party size, ask for it next
        if (!dc.activeDialog.state.partySize) {
            await dc.prompt('textPrompt', "How many people are in your party?");
        } else {
            await next();
        }
    },
    async function (dc, result, next) {
        if (!dc.activeDialog.state.partySize) {
            dc.activeDialog.state.partySize = result;
        }
        // Ask for the reservation name next
        await dc.prompt('textPrompt', "Whose name will this be under?");
    },
    async function (dc, result) {
        dc.activeDialog.state.Name = result;

        // Save data to conversation state
        var state = conversationState.get(dc.context);

        // Copy the dialog state to the conversation state
        state = dc.activeDialog.state;

        // TODO: Add in <br/>Location: ${state.cafeLocation}
        var msg = `Reservation confirmed. Reservation details:             
            <br/>Date/Time: ${state.dateTime} 
            <br/>Party size: ${state.partySize} 
            <br/>Reservation name: ${state.Name}`;

        await dc.context.sendActivity(msg);
        await dc.end();
    }
]);

// Helper function that saves any entities found in the LUIS result
// to the dialog state
async function SaveEntities(dc, typedresult) {
    // Resolve entities returned from LUIS, and save these to state
    if (typedresult.entities) {

        let datetime = typedresult.entities.datetime;

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
                        [Creator.evening]);
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
        let partysize = typedresult.entities.partySize;
        if (partysize) {
            console.log(`partysize entity detected: ${partysize}`);
            // use first partySize entity that was found in utterance
            dc.activeDialog.state.partySize = partysize[0];
        }
        let cafelocation = typedresult.entities.cafeLocation;

        if (cafelocation) {
            console.log(`location entity detected: ${cafelocation}`);
            // use first cafeLocation entity that was found in utterance
            dc.activeDialog.state.cafeLocation = cafelocation[0][0];
        }
    }
}