/*
 * Botbuilder v4 SDK - File Storage
 * 
 * This bot demonstrates how to manage a conversation state and user state with File Storage.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * 
 * 2) From VSCode, open the package.json file and
 * Update the property "main" to the name of the sample bot you want to run. 
 *    For example: "main": "manage-conversation-and-user-state/azure-table-storage.js" to run the this sample bot.
 * 3) run the bot in debug mode. 
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

const { BotFrameworkAdapter, FileStorage, ConversationState, UserState, BotStateSet } = require('botbuilder');
const restify = require('restify');


// Create server
let server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`${server.name} listening to ${server.url}`);
});

// Create adapter (it's ok for MICROSOFT_APP_ID and MICROSOFT_APP_PASSWORD to be blank for now)  
const adapter = new BotFrameworkAdapter({ 
    appId: process.env.MICROSOFT_APP_ID, 
    appPassword: process.env.MICROSOFT_APP_PASSWORD 
});

// Store in using FileStorage
const storage = new FileStorage("c:/temp");
const conversationState = new ConversationState(storage);
const userState  = new UserState(storage);
adapter.use(new BotStateSet(conversationState, userState));


// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        const convo = conversationState.get(context);
        const user = userState.get(context);

        if (isMessage) {
            if(!user.name && !convo.prompt){
                // Ask for the name.
                await context.sendActivity("What is your name?")
                // Set flag to show we've asked for the name. We save this out so the
                // context object for the next turn of the conversation can check haveAskedNameFlag
                convo.prompt = "haveAskedNameFlag";
            } else if(convo.prompt == "haveAskedNameFlag"){
                // Save the name.
                user.name = context.activity.text;
                // Ask the user for their number
                await context.sendActivity(`Hello, ${user.name}. What's your telephone number?`);
                // Set flag
                convo.prompt = "haveAskedNumberFlag";
            } else if(convo.prompt == "haveAskedNumberFlag"){
                // save the phone number
                user.telephonenumber = context.activity.text;
                convo.prompt = undefined; // Reset flag
                await context.sendActivity(`Got it. I'll call you later.`);
            }
        }

        // ...
    });
});