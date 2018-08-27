/*
 * Botbuilder v4 SDK - Using Event Handlers
 * 
 * This bot demonstrates how to use event handlers. Event handlers are functions we can add to future activity events within a turn.
 * SendActivity, UpdateActivity, and DeleteActivity are useful when you need to do something on every future activity of that type for the current context object.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * 
 * 2) From VSCode, open the package.json file and make sure that "main" is not set to any path (or is undefined) 
 * 3) Navigate to your bot app.js file and run the bot in debug mode (eg: click Debug/Start debuging)
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

const { BotFrameworkAdapter } = require('botbuilder');
const restify = require('restify');

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

// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, async (context) => {

        if (context.activity.type === 'message') {

            // This example will listen to the user's input activity when help is written.
    
            context.onSendActivities(async (handlerContext, activities, handlerNext) => { 
                
                if(handlerContext.activity.text === 'help'){
                    console.log('help!')
                    // Do whatever logging you want to do for this help message
                }
                // Add handler logic here
            
                await handlerNext(); 
            });
            await context.sendActivity(`you said ${context.activity.text}`);
        }
    });
});