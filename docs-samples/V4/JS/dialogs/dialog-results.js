/*
 * Botbuilder v4 SDK - Return results from a dialog.
 * 
 * This bot demonstrates how to return results back from a dialog object.
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

// Packages are installed for you
const { BotFrameworkAdapter, MemoryStorage, ConversationState } = require('botbuilder');
const restify = require('restify');
const {dialogs} = require('botbuilder-dialogs');

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

//
// NOTE: This sample is to show how to use dialogs library directly. However, the current library structure 
// does not allow this. So, to use dialogs, you must use DialogContext library or DialogSet library.
//
// THE SAMPLE CODE BELOW WON'T COMPILE OR RUN
//
//var dialog = new dialogs();

dialogs.add('askName', [
    function (context, next){
        context.sendActivity("What is your name?");
    },
    function (context, value){
        this.end(value);
    }
]);

// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, (context) => {
        var isMessage = (context.activity.type === 'message'); 


        // This bot is only handling Messages
        if (isMessage) {
            var state = {};
            var completion = dialogs.begin(context, state);

            if(completion.isCompleted){
                const value = completion.result;
                context.sendActivity(`The dialog return value is *${value}*.`);
            }

            completion = dialogs.continue(context, state);

        } else {
            // Echo back the type of activity the bot detected if not of type message
            return context.sendActivity(`[${context.activity.type} event detected]`);
        }
    });
});