/*
 * Botbuilder v4 SDK - Using prompts.
 * 
 * This bot demonstrates how to use each of the prompt methods that is available through
 * the 'botbuilder-dialogs' package.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --save botbuilder-dialogs@preview
 * 
 * 2) From VSCode, open the package.json file and
 * Update the property "main" to the name of the sample bot you want to run. 
 *    For example: "main": "using-prompts/app.js" to run the this sample bot.
 * 3) run the bot in debug mode. 
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

// Required packages for this bot
const { BotFrameworkAdapter, FileStorage, ConversationState, UserState, BotStateSet, MessageFactory } = require('botbuilder');
const restify = require('restify');
const { DialogSet, TextPrompt, DatetimePrompt, NumberPrompt, ChoicePrompt, AttachmentPrompt, ConfirmPrompt, OAuthPrompt } = require('botbuilder-dialogs');

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
const storage = new FileStorage("c:/temp"); // Go to this directory to verify the persisted data
const conversationState = new ConversationState(storage);
const userState  = new UserState(storage);
adapter.use(new BotStateSet(conversationState, userState));

const dialogs = new DialogSet();

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        // State will store all of your information 
        const convo = conversationState.get(context);
        const dc = dialogs.createContext(context, convo);

        if (isMessage) {
            // Check for valid intents
            if(context.activity.text && context.activity.text.match(/hi|prompts/ig)){
                await dc.begin('listPrompts');
            }
        }

        if(!context.responded){
            // Continue executing the "current" dialog, if any.
            await dc.continue();

            if(!context.responded && isMessage){
                // Default message
                await context.sendActivity("Hi! I'm a simple bot. Please say 'Hi' or 'prompts' to get started.");
            }
        }
    });
});


// List prompts:
// Provide a list of prompts to test out each prompt type

var choices = ['TextPrompt', 'NumberPrompt', 'ChoicePrompt', 'DatetimePrompt', 'ConfirmPrompt', 'AttachmentPrompt', 'OAuthPrompt']

dialogs.add('listPrompts',[
    async function (dc){
        await dc.prompt('choicePrompt', 'Which prompt type would you like to try?', choices,
            { retryPrompt: 'Invalid choice. Please pick one from the list of choices.' });
    },
    async function(dc, results){
        switch(results.value){
            case 'TextPrompt':
                await dc.begin('askName');
            break;
            case 'NumberPrompt':
                await dc.begin('askAge');
            break;
            case 'ChoicePrompt':
                await dc.begin('pickAColor');
            break;
            case 'DatetimePrompt':
                await dc.begin('setAppointment');
            break;
            case 'ConfirmPrompt':
                await dc.begin('cancelOrder');
            break;
            case 'AttachmentPrompt':
                await dc.begin('uploadImage');
            break;
            case 'OAuthPrompt':
                await dc.begin('gitSignOn');
            break;
            default:
            break;
        }
    },
    async function(dc, results){
        dc.replace('listPrompts'); // Repeat the process
    }
]);

// Define prompts
// Generic prompts
dialogs.add('textPrompt', new TextPrompt());
dialogs.add('datetimePrompt', new DatetimePrompt(datetimeValidation));
dialogs.add('numberPrompt', new NumberPrompt(ageValidation));
dialogs.add('choicePrompt', new ChoicePrompt());
dialogs.add('attachmentPrompt', new AttachmentPrompt(imageValidation));
dialogs.add('confirmPrompt', new ConfirmPrompt());
dialogs.add('oauthPrompt', new OAuthPrompt({
    connectionName: 'GitConnection',
    title: 'Login to GitHub',
    timeout: 300000 // User has 5 minutes to login before connection expires
}));

// Prompt types

// TextPrompt
// This dialog uses a textPrompt to ask the user for their name as a text string input then greet the user by name.
dialogs.add('askName', [
    async function(dc){
        await dc.prompt('textPrompt', 'What is your name?');
    },
    async function(dc, results){
        var name = results;
        await dc.context.sendActivity(`Hi ${name}!`);
        await dc.end();
    }
]);


// NumberPrompt
// This dialog uses the numberPrompt to ask user for a numerical input. The NumberPrompt can parse text or numerical input.
// If user enters text input, the NumberPrompt will convert it to a numerical value.
// For example, if user enters "ten", the prompt will convert the input to the numerical "10".
dialogs.add('askAge', [
    async function(dc){
        await dc.prompt('numberPrompt', "How old are you?");
    },
    async function(dc, results){
        var age = results;
        await dc.context.sendActivity(`You are ${age} years old.`);
        await dc.end();
    }
]);

// Age validation criteria
async function ageValidation(context, value){
    try {
        if(value < 10) {
            throw new Error(`Age too low.`);
        }
        else if(value > 100){
            throw new Error(`Age too high.`);
        }
        return value; // Return the valid value
    }
    catch (err){
        await context.sendActivity(`${err.message} Please specify an age between 10 - 100.`);
        return undefined;
    }
}

// ChoicePrompt
// This dialog uses the choicePrompt to ask user to pick from a list of choices.
dialogs.add('pickAColor', [
    async function(dc){
        var colors = ['red', 'green', 'orange', 'yellow'];
        await dc.prompt('choicePrompt', "What color would you like?", colors, 
        {retryPrompt: 'Invalid color. Please choose a color from the list.'});
    },
    async function(dc, results){
        var colorChoice = results.value;
        await dc.context.sendActivity(`${colorChoice} is a great color!`);
        await dc.end();
    }
]);

// DatetimePrompt
// This dialog uses the datatimePrompt to ask the user for a date and time
dialogs.add('setAppointment', [
    async function(dc){
        await dc.prompt('datetimePrompt', "When would you like to set the appointment for? Please specify a date and time (e.g.: tomorrow at 9am)");
    },
    async function(dc, results){
        var datetime = results;
        await dc.context.sendActivity(`Ok. Your appointment is set for ${datetime}.`);
        await dc.end();
    }
]);

// datetime validation criteria
async function datetimeValidation(context, values){
    try {
        if (!Array.isArray(values) || values.length < 0) { throw new Error('Missing time') }
        if (values[0].type !== 'datetime') { throw new Error('Unsupported type') }
        const value = new Date(values[0].value);
        if (value.getTime() < new Date().getTime()) { throw new Error('In the past') }
        return value; // Return the valid date time values
    } catch (err) {
        await context.sendActivity(`${err.message}. Answer with a time in the future like "tomorrow at 9am".`);
        return undefined;
    }
}

// ConfirmPrompt
// This dialog uses the confirmPrompt to ask user to confirm their choices with a yes/no response.
// The ConfirmPrompt will return a boolean representing the user's selection.
dialogs.add('cancelOrder', [
    async function(dc){
        await dc.prompt('confirmPrompt', 'This is cancel your order. Are you sure?', 
        {retryPrompt: 'Please answer "yes" or "no".'});
    },
    async function(dc, results){
        var isConfirmed = results;
        if(isConfirmed){
            await dc.context.sendActivity(`Ok. Your order has been cancelled.`);
        }
        else {
            await dc.context.sendActivity(`No problem. We will continue with your ordering process.`);
        }
        await dc.end();
    }
]);

// AttachmentPrompt
// This dialog uses the attachmentPrompt to ask user for a file attachment; usually, an image.
// The user can send a single image or a list of images.
dialogs.add('uploadImage', [
    async function(dc){
        await dc.prompt('attachmentPrompt', 'Please choose images to upload?');
    },
    async function(dc, results){
        var images = results;
        await dc.context.sendActivity( MessageFactory.list(images, 'Image uploaded.') );
        await dc.end();
    }
]);

// This validation only allow images to be attached.
async function imageValidation(context, values){
    if (values && values.length > 0) {
        for (let i = 0; i < values.length; i++) {
           if (!values[i].contentType.startsWith('image')) {
              await context.sendActivity(`Only images are accepted. Please try again.`);
              return undefined;
           }
        }
     } else {
        await context.sendActivity(`Please upload at least one image.`);
     }
     return values;
}


// OAuthPrompt
// This dialog uses the oauthPrompt to ask the user to sign in using the Bot Framework "Single Sign On (SSO)" service.
dialogs.add('gitSignOn', [
    async function(dc){
        await dc.prompt('oauthPrompt');
    },
    async function(dc, results){
        var token = results;
        if(token){
            // continue processing access token
        }
        else {
            await dc.context.sendActivity(`Sorry, we can't sign you in. Please try again later.`)
            await dc.end();
        }
    }
]);

const gitConnection = {
    connectionName: 'GitConnection',
    title: 'Login to GitHub',
    timeout: 300000 // User has 5 minutes to login before connection expires
}