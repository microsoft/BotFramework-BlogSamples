/*
 * Botbuilder V4 SDK - Single Conversation Flow using Topic Flags
 * (No Botbuilder dialogs or prompts abstraction libraries)
 * 
 * This sample demonstrates how to manually manage a directed conversation flow.
 * This sample will ask the user a few questions about themselves and greet them by name if their profile exists.
 * Information will be stored to the "ConversationState" storage bag.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --save @microsoft/recognizers-text-suite
 * 
 * 2) From VSCode, open the package.json file and
 * Update the property "main" to the name of the sample bot you want to run. 
 *    For example: "main": "primitive-prompts/single-conversation.js" to run the this sample bot.
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
const storage = new MemoryStorage(); // Volatilve memory
const conversationState = new ConversationState(storage);
const userState  = new UserState(storage);
adapter.use(new BotStateSet(conversationState, userState));

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        // State will store all of your information 
        const convo = conversationState.get(context);

        // Defined flags to manage the conversation flow and prompt states
        // convo.topicTitle - Current conversation topic in progress
        // convo.prompt - Current prompt state in progress - indicate what question is being asked.
        
        if (isMessage) {
            // Defile a topicStates object if it doesn't exist in the convoState.
            if(!convo.topicStates){
                convo.topicStates = {
                    "topicTitle": undefined, // Current conversation topic in progress
                    "prompt": undefined      // Current prompt state in progress - indicate what question is being asked.
                };
            }
            
            // If user profile is not defined then define it.
            if(!convo.userProfile){
                
                await context.sendActivity(`Welcome new user, please fill out your profile information.`);
                convo.topicStates.topicTitle = "profileTopic"; // Start the userProfile topic
                convo.userProfile = { // Define the user's profile object
                    "userName": undefined,
                    "age": undefined,
                    "workPlace": undefined
                }; 
            }

            // Start or continue a conversation if we are in one
            if(convo.topicStates.topicTitle == "profileTopic"){
                await gatherUserProfile(context, convo);
            }

            // Check for valid intents
            else if(context.activity.text && context.activity.text.match(/hi/ig)){
                await context.sendActivity(`Hi ${convo.userProfile.userName}.`);
            }
            else {
                // Default message
                await context.sendActivity("Hi. I'm the Contoso bot.");
            }
        }

    });
});

// Ask the user for their profile information
async function gatherUserProfile(context, convo){
    if(!convo.userProfile.userName && !convo.topicStates.prompt){
        convo.topicStates.prompt = "askName";
        await context.sendActivity("What is your name?");
    }
    else if(convo.topicStates.prompt == "askName"){
        // Save the user's response
        convo.userProfile.userName = context.activity.text; 

        // Ask next question
        convo.topicStates.prompt = "askAge";
        await context.sendActivity("How old are you?");
    }
    else if(convo.topicStates.prompt == "askAge"){
        // Save user's response
        convo.userProfile.age = context.activity.text;

        // Ask next question
        convo.topicStates.prompt = "workPlace";
        await context.sendActivity("Where do you work?");
    }
    else if(convo.topicStates.prompt == "workPlace"){
        // Save user's response
        convo.userProfile.workPlace = context.activity.text;

        // Done
        convo.topicStates.topicTitle = undefined; // Reset conversation flag
        convo.topicStates.prompt = undefined;     // Reset the prompt flag
        await context.sendActivity("Thank you. Your profile is complete.");
    }

}