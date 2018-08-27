/*
 * Botbuilder v4 SDK - Add Two Numbers.
 * 
 * This bot demonstrates how to use a single step waterfall. In this case, the dialog simply
 * add two numbers together whenever the user says something like, "What's 2+3?".
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
const { DialogSet, WaterfallDialog } = require('botbuilder-dialogs');

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
const userState  = new UserState(storage);
adapter.use(new BotStateSet(conversationState, userState));

const dialogs = new DialogSet(conversationState.createProperty('dialogState'));

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    // Route received activity to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        const isMessage = context.activity.type === 'message';
        // State will store all of your information 
        const convoState = conversationState.get(context);
        const dc = await dialogs.createContext(context);

        if (isMessage) {
            // TryParseAddingTwoNumbers checks if the message matches a regular expression
            // and if it does, returns an array of the numbers to add
            var numbers = await TryParseAddingTwoNumbers(context.activity.text); 
            if (numbers != null && numbers.length >=2 )
            {    
                var sum = await dc.begin('addTwoNumbers', numbers); // A sum is returned
                // do something with sum if desired
                console.log("sum is: " + sum.result);
            }
            else {
                await dc.context.sendActivity(`Hi! I'm the add 2 numbers bot. Say something like "What's 2+3?"`);
            }     
        }
        else {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }
        
    });
});

// Add two numbers
// This sample shows a single step waterfall in a processing an addition equation.

// Show the sum of two numbers.
dialogs.add(new WaterfallDialog('addTwoNumbers', [async function (dc, step){
    var sum = Number.parseFloat(step.options[0]) + Number.parseFloat(step.options[1]);
    await dc.context.sendActivity(`${step.options[0]} + ${step.options[1]} = ${sum}`);
    return await dc.end(sum); // return the sum
}]
));

async function TryParseAddingTwoNumbers(message) {
    const ADD_NUMBERS_REGEXP = /([-+]?(?:[0-9]+(?:\.[0-9]+)?|\.[0-9]+))(?:\s*)\+(?:\s*)([-+]?(?:[0-9]+(?:\.[0-9]+)?|\.[0-9]+))/i;
    let matched = ADD_NUMBERS_REGEXP.exec(message);
    if (!matched) {
        // message wasn't a request to add 2 numbers
        return null;
    }
    else {
        var numbers = [matched[1], matched[2]];
        return numbers;
    }
}