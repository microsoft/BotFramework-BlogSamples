/*
 * Botbuilder v4 SDK - Simple Conversation Flows.
 * 
 * This bot demonstrates how to use dialogs, waterfall, and prompts to manage conversation flows.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --save botbuilder-dialogs@preview
 * 
 * 2) From VSCode, open the package.json file and make sure that "main" is not set to any path (or is undefined) 
 * 3) Navigate to your bot app.js file and run the bot in debug mode (eg: click Debug/Start debuging)
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

// Required packages for this bot
const { BotFrameworkAdapter, MemoryStorage, ConversationState, UserState, BotStateSet } = require('botbuilder');
const restify = require('restify');
const { DialogSet, WaterfallDialog, TextPrompt, DateTimePrompt, NumberPrompt, ChoicePrompt } = require('botbuilder-dialogs');

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

// Storage
const storage = new MemoryStorage(); // Volatile memory
const conversationState = new ConversationState(storage);
const userInfoAccessor = conversationState.createProperty('userInfo');
adapter.use(new BotStateSet(conversationState));

// Define a dialog set with state object set to the conversation state.
const dialogs = new DialogSet(conversationState.createProperty('dialogState'));

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        //const convoState = await conversationState.get(context);
        const dc = await dialogs.createContext(context);
        
        if (isMessage) {
            // Check for valid intents
            if(context.activity.text.match(/hello/ig)){
                return await dc.begin('greetings');
            }
        }

        if(!context.responded){
            // Continue executing the "current" dialog, if any.
            var results = await dc.continue();

            if(results.status == "complete"){
                // Do something with `results.result`
                // ...
            }

            if(!context.responded && isMessage){
                // Default message
                await context.sendActivity("Hi! I'm a simple bot. Please say 'Hello'.");
            }
        }
    });
});


// Greet user:
// Ask for the user name and then greet them by name.
// Ask them where they work.
dialogs.add(new WaterfallDialog('greetings', [
    async function (dc, step){
        step.values.userInfo = {}; // New object
        return await dc.prompt('textPrompt', 'Hi! What is your name?');
    },
    async function(dc, step){
        var userName = step.result;
        step.values.userInfo.userName = userName;
        await dc.context.sendActivity(`Hi ${userName}!`);
        return await dc.prompt('textPrompt', 'Where do you work?');
    },
    async function(dc, step){
        var workPlace = step.result;
        step.values.userInfo.workPlace = workPlace;
        await dc.context.sendActivity(`${workPlace} is a fun place.`);

        // Persist user data
        const userData = await userInfoAccessor.get(dc.context, {});
        userData.userInfo = step.values.userInfo;

        return await dc.end(); // Ends the dialog
    }
]));

// Define prompts
// Generic prompts
dialogs.add(new TextPrompt('textPrompt'));