/*
 * Botbuilder v4 SDK - Persist user data.
 * 
 * This bot demonstrates how to persist user input to a volatile "in-memory" as storage source. 
 * This bot will ask user for 'reserve table' information. The information collected are 
 * stored temporarily in the dialog's `step.values.reserveInfo` object. Then, it is persisted
 * to the conversation state at the end of the waterfall.
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
const restify = require('restify');
const { BotFrameworkAdapter, MemoryStorage, ConversationState, UserState, BotStateSet } = require('botbuilder');
const { DialogSet, WaterfallDialog, TextPrompt, DateTimePrompt, NumberPrompt } = require('botbuilder-dialogs');
const { CosmosDbStorage } = require('botbuilder-azure');

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

// const storage = new CosmosDbStorage({
//     serviceEndpoint: "https://lucascosmos.documents.azure.com:443/", 
//     authKey: "xCmY4dZqZACCVUoExxAbfa38MwPjShqbm2TTqNhzKnqlTCLSc8mTio1TeCbEu2dCCg8VmNAohRARuUMPmmrRPg==", 
//     databaseId: "Tasks",
//     collectionId: "Items"
// });

// const storage = new CosmosDbStorage({
//     serviceEndpoint: process.env.ACTUAL_SERVICE_ENDPOINT, 
//     authKey: process.env.ACTUAL_AUTH_KEY, 
//     databaseId: process.env.DATABASE,
//     collectionId: process.env.COLLECTION
// });

const conversationState = new ConversationState(storage);
const userState  = new UserState(storage);
const userDataAccessor = userState.createProperty('userData');

adapter.use(new BotStateSet(conversationState, userState));

const dialogs = new DialogSet(conversationState.createProperty('dialogState')); // Must provide a state object

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        //const isMessage = (context.activity.type === 'message');
        // State will store all of your information 
        const dc = await dialogs.createContext(context);
        const userData = await userDataAccessor.get(context, {});

        switch(context.activity.type){
            case "conversationUpdate":
                if(context.activity.membersAdded[0].id != context.activity.recipient.id){
                    if(!userData.name){
                        // If we don't already have their name, start a dialog to collect it.
                        await context.sendActivity("Welcome to the User Data bot.");
                        return await dc.begin('greetings');
                    }
                    else {
                        // Otherwise, greet them by name.
                        await context.sendActivity(`Hi ${userData.name}! Welcome back to the User Data bot.`);
                    }
                }
                break;
            case "message":
                var turnResult = await dc.continue();
                if(turnResult.status == "complete"){
                    // If it completes successfully and returns a valid name, save the name and greet the user.
                    userData.name = turnResult.result;
                    await context.sendActivity(`Pleased to meet you ${userData.name}.`);
                }
                // Else, if we don't have the user's name yet, ask for it.
                else if(!userData.name){
                    return await dc.begin('greetings');
                }
                // Else, echo the user's message text.
                else {
                    await context.sendActivity(`${userData.name} said, ${context.activity.text}.`);
                }
                break;
            case "deleteUserData":
                // Delete the user's data.
                // Note: You can use the emuluator to send this activity.
                userData.Name = null;
                await context.sendActivity("I have deleted your user data.");
                break;
        }
    });
});


// Greet user:
// Ask for the user name and then greet them by name.
dialogs.add(new WaterfallDialog('greetings', [
    async function (dc, step){
        return await dc.prompt('textPrompt', 'Hi! What is your name?');
    },
    async function(dc, step){
        // step.values.userName = step.result;
        // await dc.context.sendActivity(`Hi ${step.values.userName}!`);
        
        // // Persist user data to user state
        // const userState = await userInfoAccessor.get(dc.context, {});
        // userState.userInfo = step.values;

        // Assume that they entered their name, and return the value.
        return await dc.end(step.result);
    }
]));

// Define prompts
// Generic prompts
dialogs.add(new TextPrompt('textPrompt'));
dialogs.add(new DateTimePrompt('dateTimePrompt'));
dialogs.add(new NumberPrompt('partySizePrompt'));