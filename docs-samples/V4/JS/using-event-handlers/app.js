/*
 * Botbuilder v4 SDK - Using Event Handlers
 * 
 * This bot demonstrates how to use the sendActivity event handler
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * 
 * 2) From VSCode, open the package.json file and
 * Update the property "main" to the name of the sample bot you want to run. 
 *    For example: "main": "using-event-handlers/app.js" to run the this sample bot.
 * 3) run the bot in debug mode. 
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