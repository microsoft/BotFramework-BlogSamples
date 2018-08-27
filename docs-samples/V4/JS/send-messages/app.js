/*
 * Botbuilder v4 SDK - Send messages
 * 
 * This bot demonstrates how to send a simple text message with context.sendActivity()
 * If you would like to add inputHints and enable speech, use MessageFactory.text() instead. 
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

// Packages are installed for you
const { BotFrameworkAdapter, MemoryStorage, ConversationState, MessageFactory } = require('botbuilder');
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

// Add conversation state middleware
const conversationState = new ConversationState(new MemoryStorage());
adapter.use(conversationState);

// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        // This bot is only handling Messages
        if (context.activity.type === 'message') {
            
            // Send a simple text message
            await context.sendActivity("Greetings from sample message.");

            // Send a spoken message if speach is enabled 
            const basicMessage = MessageFactory.text('This is the text that will be displayed.', 'This is the text that will be spoken.');
            await context.sendActivity(basicMessage);
        } 
    });
});