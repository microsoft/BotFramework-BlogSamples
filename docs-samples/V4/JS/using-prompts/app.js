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
 * 2) From VSCode, open the package.json file and make sure that "main" is not set to any path (or is undefined) 
 * 3) Navigate to your bot app.js file and run the bot in debug mode (eg: click Debug/Start debuging)
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

// Required packages for this bot
const { BotFrameworkAdapter, MemoryStorage, ConversationState, UserState, BotStateSet, MessageFactory } = require('botbuilder');
const restify = require('restify');
const { DialogSet, WaterfallDialog, TextPrompt, DateTimePrompt, NumberPrompt, ChoicePrompt, AttachmentPrompt, ConfirmPrompt, OAuthPrompt } = require('botbuilder-dialogs');

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

const dialogs = new DialogSet(conversationState.createProperty('dialogState'));

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        // State will store all of your information 
        const dc = await dialogs.createContext(context);

        if (isMessage) {
            // Check for valid intents
            if(context.activity.text && context.activity.text.match(/hello|prompts/ig)){
                return await dc.begin('listPrompts');
            }
        }

        if(!context.responded){
            // Continue executing the "current" dialog, if any.
            await dc.continue();

            if(!context.responded && isMessage){
                // Default message
                await context.sendActivity("Hi! I'm a simple Prompt bot. Please say 'Hello' or 'prompts' to get started.");
            }
        }
    });
});


// List prompts:
// Provide a list of prompts to test out each prompt type

var choices = ['TextPrompt', 'NumberPrompt', 'ChoicePrompt', 'DatetimePrompt', 'ConfirmPrompt', 'AttachmentPrompt', 'OAuthPrompt']

dialogs.add(new WaterfallDialog('listPrompts',[
    async function (dc, step){
        return await dc.prompt('choicePrompt', 'Which prompt type would you like to try?', choices,
            { retryPrompt: 'Invalid choice. Please pick one from the list of choices.' });
    },
    async function(dc, step){
        switch(step.result.value){
            case 'TextPrompt':
                return await dc.begin('askName');
            break;
            case 'NumberPrompt':
                return await dc.begin('askAge');
            break;
            case 'ChoicePrompt':
                return await dc.begin('pickAColor');
            break;
            case 'DatetimePrompt':
                return await dc.begin('setAppointment');
            break;
            case 'ConfirmPrompt':
                return await dc.begin('cancelOrder');
            break;
            case 'AttachmentPrompt':
                return await dc.begin('uploadImage');
            break;
            case 'OAuthPrompt':
                return await dc.begin('gitSignOn');
            break;
            default:
            break;
        }
    },
    async function(dc, step){
        return await dc.replace('listPrompts'); // Repeat the process
    }
]));

// Define prompts
// Generic prompts
dialogs.add(new TextPrompt('textPrompt'));
dialogs.add(new DateTimePrompt('datetimePrompt', datetimeValidator));
dialogs.add(new NumberPrompt('numberPrompt', ageValidator));
dialogs.add(new ChoicePrompt('choicePrompt'));
dialogs.add(new AttachmentPrompt('attachmentPrompt', imageValidator));
dialogs.add(new ConfirmPrompt('confirmPrompt'));
dialogs.add(new OAuthPrompt('oauthPrompt', {
    connectionName: 'GitConnection',
    title: 'Login to GitHub',
    timeout: 300000 // User has 5 minutes to login before connection expires
}));

// Prompt types

// TextPrompt
// This dialog uses a textPrompt to ask the user for their name as a text string input then greet the user by name.
dialogs.add(new WaterfallDialog('askName', [
    async function(dc, step){
        return await dc.prompt('textPrompt', 'What is your name?');
    },
    async function(dc, step){
        var name = step.result;
        await dc.context.sendActivity(`Hi ${name}!`);
        return await dc.end();
    }
]));


// NumberPrompt
// This dialog uses the numberPrompt to ask user for a numerical input. The NumberPrompt can parse text or numerical input.
// If user enters text input, the NumberPrompt will convert it to a numerical value.
// For example, if user enters "ten", the prompt will convert the input to the numerical "10".
dialogs.add(new WaterfallDialog('askAge', [
    async function(dc, step){
        return await dc.prompt('numberPrompt', "How old are you?");
    },
    async function(dc, step){
        var age = step.result;
        await dc.context.sendActivity(`You are ${age} years old.`);
        return await dc.end();
    }
]));

// Age validation criteria
async function ageValidator(context, promptValidatorContext){
    var msg = null;
    value = promptValidatorContext.recognized.value; // Get the user's input value
    if(value < 10) {
        msg = `Age too low.`;
    }
    else if(value > 100){
        msg = `Age too high.`;
    }

    if(msg){ // fail
        msg += " Please enter a number between 10 and 100."
        await context.sendActivity(msg);
    }
    else{ // pass
        return await promptValidatorContext.end(value); // end the prompt and past the value back
    }
    
}

// ChoicePrompt
// This dialog uses the choicePrompt to ask user to pick from a list of choices.
dialogs.add(new WaterfallDialog('pickAColor', [
    async function(dc, step){
        var colors = ['red', 'green', 'orange', 'yellow'];
        return await dc.prompt('choicePrompt', "What color would you like?", colors, 
        {retryPrompt: 'Invalid color. Please choose a color from the list.'});
    },
    async function(dc, step){
        var colorChoice = step.result.value;
        await dc.context.sendActivity(`${colorChoice} is a great color!`);
        return await dc.end();
    }
]));

// DatetimePrompt
// This dialog uses the datatimePrompt to ask the user for a date and time
dialogs.add(new WaterfallDialog('setAppointment', [
    async function(dc, step){
        return await dc.prompt('datetimePrompt', "When would you like to set the appointment for? Please specify a date and time (e.g.: tomorrow at 9am)");
    },
    async function(dc, step){
        var datetime = step.result;
        await dc.context.sendActivity(`Ok. Your appointment is set for ${datetime[0].value}.`);
        return await dc.end();
    }
]));

// datetime validation criteria
async function datetimeValidator(context, promptValidatorContext){
    var msg = "";
    values = promptValidatorContext.recognized.value; // Get the user input's value
    if (!Array.isArray(values) || values.length < 0) { 
        msg += 'Missing time.\n ';
    }
    if (values[0].type !== 'datetime') { 
        msg += 'Unsupported type.\n ';
    }
    
    const value = new Date(values[0].value);
    if (value.getTime() < new Date().getTime()) { 
        msg += 'In the past.\n '
    }
    
    if(msg){ // fail
        msg += " Please enter a validate date and time (e.g.: tomorrow at 3pm)."
        await context.sendActivity(msg);
    }
    else{ // pass
        return await promptValidatorContext.end(values); // end the prompt and past the value back
    }
}

// ConfirmPrompt
// This dialog uses the confirmPrompt to ask user to confirm their choices with a yes/no response.
// The ConfirmPrompt will return a boolean representing the user's selection.
dialogs.add(new WaterfallDialog('cancelOrder', [
    async function(dc, step){
        return await dc.prompt('confirmPrompt', 'This is cancel your order. Are you sure?', 
        {retryPrompt: 'Please answer "yes" or "no".'});
    },
    async function(dc, step){
        var isConfirmed = step.result;
        if(isConfirmed){
            await dc.context.sendActivity(`Ok. Your order has been cancelled.`);
        }
        else {
            await dc.context.sendActivity(`No problem. We will continue with your ordering process.`);
        }
        return await dc.end();
    }
]));

// AttachmentPrompt
// This dialog uses the attachmentPrompt to ask user for a file attachment; usually, an image.
// The user can send a single image or a list of images.
dialogs.add(new WaterfallDialog('uploadImage', [
    async function(dc, step){
        return await dc.prompt('attachmentPrompt', 'Please choose images to upload?');
    },
    async function(dc, step){
        var images = step.result;
        await dc.context.sendActivity( MessageFactory.list(images, 'Image uploaded.') );
        return await dc.end();
    }
]));

// This validation only allow images to be attached.
async function imageValidator(context, promptValidatorContext){
    var values = promptValidatorContext.recognized.value;
    var msg = "";

    if (values && values.length > 0) {
        for (let i = 0; i < values.length; i++) {
           if (!values[i].contentType.startsWith('image')) {
              msg += `Only images are accepted. Please try again.`;
           }
        }
    } else {
        msg += `Please upload at least one image.`;
    }

    if(msg){ // fail
        await context.sendActivity(msg);
    }
    else{ // pass
        return await promptValidatorContext.end(values); // end the prompt and past the value back
    }
}


// OAuthPrompt
// This dialog uses the oauthPrompt to ask the user to sign in using the Bot Framework "Single Sign On (SSO)" service.
dialogs.add(new WaterfallDialog('gitSignOn', [
    async function(dc, step){
        return await dc.prompt('oauthPrompt');
    },
    async function(dc, step){
        var token = step.result;
        if(token){
            // continue processing access token
        }
        else {
            await dc.context.sendActivity(`Sorry, we can't sign you in. Please try again later.`)
            return await dc.end();
        }
    }
]));

const gitConnection = {
    connectionName: 'GitConnection',
    title: 'Login to GitHub',
    timeout: 300000 // User has 5 minutes to login before connection expires
}