"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const botbuilder_1 = require("botbuilder");
const botbuilder_dialogs_1 = require("botbuilder-dialogs");
const botbuilder_ai_1 = require("botbuilder-ai");
const restify = require("restify");
const Recognizers = require("@microsoft/recognizers-text-date-time");
// This App ID is for the cafebot public LUIS app
const appId = "edaadd9b-b632-4733-a25c-5b67271035dd";
const subscriptionKey = "be30825b782843dcbbe520ac5338f567";
// Default is westus
const serviceEndpoint = 'https://westus.api.cognitive.microsoft.com';
const luisRec = new botbuilder_ai_1.LuisRecognizer({
    appId: appId,
    subscriptionKey: subscriptionKey,
    serviceEndpoint: serviceEndpoint
});
// Enum for convenience
// intent names match CafeLUISModel.ts
var Intents;
(function (Intents) {
    Intents["Book_Table"] = "Book_Table";
    Intents["Greeting"] = "Greeting";
    Intents["None"] = "None";
    Intents["Who_are_you_intent"] = "Who_are_you_intent";
})(Intents || (Intents = {}));
;
// Create server
let server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`${server.name} listening to ${server.url}`);
});
// Create adapter
const adapter = new botbuilder_1.BotFrameworkAdapter({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});
const conversationState = new botbuilder_1.ConversationState(new botbuilder_1.MemoryStorage());
adapter.use(conversationState);
// Create empty dialog set
const dialogs = new botbuilder_dialogs_1.DialogSet();
// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, (context) => __awaiter(this, void 0, void 0, function* () {
        const isMessage = context.activity.type === 'message';
        // Create dialog context 
        const state = conversationState.get(context);
        const dc = dialogs.createContext(context, state);
        if (!isMessage) {
            yield context.sendActivity(`[${context.activity.type} event detected]`);
        }
        // Check to see if anyone replied. 
        if (!context.responded) {
            yield dc.continue();
            // if the dialog didn't send a response
            if (!context.responded && isMessage) {
                yield luisRec.recognize(context).then((res) => __awaiter(this, void 0, void 0, function* () {
                    var typedresult = res;
                    let topIntent = botbuilder_ai_1.LuisRecognizer.topIntent(res);
                    switch (topIntent) {
                        case Intents.Book_Table: {
                            yield context.sendActivity("Top intent is Book_Table ");
                            yield dc.begin('reserveTable', typedresult);
                            break;
                        }
                        case Intents.Greeting: {
                            yield context.sendActivity("Top intent is Greeting");
                            break;
                        }
                        case Intents.Who_are_you_intent: {
                            yield context.sendActivity("Top intent is Who_are_you_intent");
                            break;
                        }
                        default: {
                            yield dc.begin('default', topIntent);
                            break;
                        }
                    }
                }), (err) => {
                    // there was some error
                    console.log(err);
                });
            }
        }
    }));
});
// Add dialogs
dialogs.add('default', [
    function (dc, args) {
        return __awaiter(this, void 0, void 0, function* () {
            const state = conversationState.get(dc.context);
            yield dc.context.sendActivity(`Hi! I'm the Contoso Cafe reservation bot. Say something like make a reservation."`);
            yield dc.end();
        });
    }
]);
dialogs.add('textPrompt', new botbuilder_dialogs_1.TextPrompt());
dialogs.add('dateTimePrompt', new botbuilder_dialogs_1.DatetimePrompt());
dialogs.add('reserveTable', [
    function (dc, args, next) {
        return __awaiter(this, void 0, void 0, function* () {
            var typedresult = args;
            // Call a helper function to save the entities in the LUIS result
            // to dialog state
            yield SaveEntities(dc, typedresult);
            yield dc.context.sendActivity("Welcome to the reservation service.");
            if (dc.activeDialog.state.dateTime) {
                yield next();
            }
            else {
                yield dc.prompt('dateTimePrompt', "Please provide a reservation date and time.");
            }
        });
    },
    function (dc, result, next) {
        return __awaiter(this, void 0, void 0, function* () {
            if (!dc.activeDialog.state.dateTime) {
                // Save the dateTimePrompt result to dialog state
                dc.activeDialog.state.dateTime = result[0].value;
            }
            // If we don't have party size, ask for it next
            if (!dc.activeDialog.state.partySize) {
                yield dc.prompt('textPrompt', "How many people are in your party?");
            }
            else {
                yield next();
            }
        });
    },
    function (dc, result, next) {
        return __awaiter(this, void 0, void 0, function* () {
            if (!dc.activeDialog.state.partySize) {
                dc.activeDialog.state.partySize = result;
            }
            // Ask for the reservation name next
            yield dc.prompt('textPrompt', "Whose name will this be under?");
        });
    },
    function (dc, result) {
        return __awaiter(this, void 0, void 0, function* () {
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
            yield dc.context.sendActivity(msg);
            yield dc.end();
        });
    }
]);
// Helper function that saves any entities found in the LUIS result
// to the dialog state
function SaveEntities(dc, typedresult) {
    return __awaiter(this, void 0, void 0, function* () {
        // Resolve entities returned from LUIS, and save these to state
        if (typedresult.entities) {
            console.log(`typedresult.entities exists.`);
            let datetime = typedresult.entities.datetime;
            //console.log(datetime.toString());
            if (datetime) {
                console.log(`datetime entity defined of type ${datetime[0].type}.`);
                datetime[0].timex.forEach((value, index) => {
                    console.log(`Timex[${index}]=${value}`);
                });
                // Use the first date or time found in the utterance
                var timexValue;
                if (datetime[0].timex) {
                    timexValue = datetime[0].timex[0];
                    // More information on timex can be found here: 
                    // http://www.timeml.org/publications/timeMLdocs/timeml_1.2.1.html#timex3                                
                    // More information on the library which does the recognition can be found here: 
                    // https://github.com/Microsoft/Recognizers-Text
                    // try to see if recognizers library can parse a timex
                    const results = Recognizers.recognizeDateTime(timexValue, dc.context.activity.locale);
                    const values = results.length > 0 && results[0].resolution ? results[0].resolution.values : undefined;
                    var dtValue;
                    var dtResult;
                    if (values) {
                        dtResult = values[0];
                        dtValue = values[0].value;
                    }
                    if (datetime[0].type === "datetime") {
                        if (dtValue && dtResult.type === "datetime") {
                            dc.activeDialog.state.dateTime = dtValue;
                        }
                        else {
                            // use original timex format if recognizers couldn't parse a datetime
                            dc.activeDialog.state.dateTime = timexValue;
                        }
                    }
                    else {
                        // TODO: also handle existence of state.date and state.time
                        console.log(`Type ${datetime[0].type} is not yet supported. Provide both the date and the time.`);
                    }
                }
            }
            let partysize = typedresult.entities.partySize;
            if (partysize) {
                console.log(`partysize entity defined.${partysize}`);
                // use first partySize entity that was found in utterance
                dc.activeDialog.state.partySize = partysize[0];
            }
            let cafelocation = typedresult.entities.cafeLocation;
            if (cafelocation) {
                console.log(`location entity defined.${cafelocation}`);
                // use first cafeLocation entity that was found in utterance
                dc.activeDialog.state.cafeLocation = cafelocation[0][0];
            }
        }
    });
}
//# sourceMappingURL=luisbot.js.map