/*
 * Botbuilder V4 SDK - Text recognizers
 * 
 * This sample demonstrates how to use the @Microsoft.Recognizers.Text for JS. 
 * The bot will use the library to recognize text in a basic math equation and evaluates it.
 * More info about this library here: 
 * https://github.com/Microsoft/Recognizers-Text/tree/master/JavaScript/packages/recognizers-text-suite
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --save botbuilder-dialogs@preview
 * npm install --save @microsoft/recognizers-text-suite
 * 
 * 2) From VSCode, open the package.json file and
 * Update the property "main" to the name of the sample bot you want to run. 
 *    For example: "main": "recognizers/text-recognizers.js" to run the this sample bot.
 * 3) run the bot in debug mode. 
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

// Required packages for this bot
const { BotFrameworkAdapter, MemoryStorage, ConversationState, UserState, BotStateSet, MessageFactory } = require('botbuilder');
const restify = require('restify');
var Recognizers = require('@microsoft/recognizers-text-suite');

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
// [NOTE: This bot does not persist data]
const storage = new MemoryStorage(); // Volatile storage
const conversationState = new ConversationState(storage);
const userState  = new UserState(storage);
adapter.use(new BotStateSet(conversationState, userState));

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        // State will store all of your information 
        const convo = conversationState.get(context);
        
        if (isMessage) {
            // Check for valid intents
            if(context.activity.text && context.activity.text.match(/hi/ig)){
                await context.sendActivity("Hi! I'm a text recognizer bot. Give me a basic math expressions and I'll tell you the answer. (e.g.: two * five)")
            }
            else {
                // Does basic math (*, /, -, +) with two numbers.
                var exp = await mathExpression(context.activity.text); 
                await context.sendActivity(`${exp[0]}${exp[2]}${exp[1]}=${eval(exp[0]+exp[2]+exp[1])}`);
                                
            }
        }

    });
});

// This method looks for a basic math expression with two numbers
// E.g.: three + two, five - two, six * four, eight / two...
// Defaults to addition if no arithmetic symbol is found.
// Returns an array
async function mathExpression(text){
    var numbers = await Recognizers.recognizeNumber(text, Recognizers.Culture.English);
    if(numbers.length > 0){
        var num = [];
        for(var i = 0; i < numbers.length; i++){
            num[i] = numbers[i].resolution.value;
        }

        if(text.includes("*")){
            num[num.length] = "*";
        }
        else if(text.includes("/")){
            num[num.length] = "/";
        }
        else if(text.includes("-")){
            num[num.length] = "-";
        }
        else {
            num[num.length] = "+";
        }
        return num;
    }
}