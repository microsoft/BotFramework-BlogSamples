/*
 * Botbuilder v4 SDK - Ask user questions.
 * 
 * This bot demonstrates how to ask user questions using Prompts and Waterfall model in a dialog.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --savea botbuilder-dialogs@preview
 * 
 * 2) From VSCode, open the package.json file and
 * Update the property "main" to the name of the sample bot you want to run. 
 *    For example: "main": "ask-questions/app.js" to run the this sample bot.
 * 3) run the bot in debug mode. 
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" or "reserve table" to engage with the bot.
 *
 */ 

// Packages are installed for you
const { BotFrameworkAdapter, MemoryStorage, ConversationState, BotStateSet } = require('botbuilder');
const restify = require('restify');
const { DialogSet, TextPrompt, DatetimePrompt, NumberPrompt } = require('botbuilder-dialogs');

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
const storage = new MemoryStorage(); // Volatile memory
const conversationState = new ConversationState(storage);
adapter.use(new BotStateSet(conversationState));

const dialogs = new DialogSet();

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        // State will store all of your information 
        const convoState = conversationState.get(context);
        const dc = dialogs.createContext(context, convoState);

        if (isMessage) {
            // Check for valid intents
            if(context.activity.text.match(/hi/ig)){
                await dc.begin('greetings');
            }
            else if(context.activity.text.match(/reserve table/ig)){
                await dc.begin('reserveTable');
            }
        }

        if(!context.responded){
            // Continue executing the "current" dialog, if any.
            await dc.continue();

            if(!context.responded && isMessage){
                // Default message
                await context.sendActivity("Hi! I'm a simple bot. Please say 'Hi' or 'reserve table'.");
            }
        }
    });
});


// Greet user:
// Ask for the user name and then greet them by name.
dialogs.add('greetings',[
    async function (dc){
        await dc.prompt('textPrompt', 'What is your name?');
    },
    async function(dc, results){
        var userName = results;
        await dc.context.sendActivity(`Hello ${userName}!`);
        await dc.end(); // Ends the dialog
    }
]);

// Reserve a table:
// Help the user to reserve a table

dialogs.add('reserveTable', [
    async function(dc, args, next){
        await dc.context.sendActivity("Welcome to the reservation service.");

        // Get state object
        convoState = conversationState.get(dc.context);
        convoState.reservationInfo = {}; // Initialize an empty object

        await dc.prompt('dateTimePrompt', "Please provide a reservation date and time.");
    },
    async function(dc, result){
        // Get state object
        convoState = conversationState.get(dc.context);
        convoState.reservationInfo.dateTime = result[0].value;

        // Ask for next info
        await dc.prompt('partySizePrompt', "How many people are in your party?");
    },
    async function(dc, result){
        // Get state object
        convoState = conversationState.get(dc.context);
        convoState.reservationInfo.partySize = result;

        // Ask for next info
        await dc.prompt('textPrompt', "Whose name will this be under?");
    },
    async function(dc, result){
        // Get state object
        convoState = conversationState.get(dc.context);
        convoState.reservationInfo.reserveName = result;
        
        // Reservation confirmation
        var msg = `Reservation confirmed. Reservation details: 
            <br/>Date/Time: ${convoState.reservationInfo.dateTime} 
            <br/>Party size: ${convoState.reservationInfo.partySize} 
            <br/>Reservation name: ${convoState.reservationInfo.reserveName}`;
        await dc.context.sendActivity(msg);
        await dc.end();
    }
]);

// Define prompts
// Generic prompts
dialogs.add('textPrompt', new TextPrompt());
dialogs.add('dateTimePrompt', new DatetimePrompt());
dialogs.add('partySizePrompt', new NumberPrompt());

