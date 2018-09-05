/*
 * Botbuilder v4 SDK - Modular Bot Logic.
 * 
 * This bot demonstrates how to break bot logics into separate modules that can be included as needed.
 * This bot requires three modules that lived in these three bot files:
 * 1) checkIn.js
 * 2) reserveTable.js
 * 3) wakeUp.js
 * 
 * Once the user says "Hi" for the first time, the bot will execute the "Check In" module. There after, the
 * bot remembers who the user is and what room they are staying in. The user can then request to either
 * "Reserve Table" or request a "Wake Up" call.
 * 
 * ------------------------------
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
const { Dialog, DialogSet, WaterfallDialog } = require('botbuilder-dialogs');

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
const userInfoState = new UserState(storage);
const userInfoAccessor = userInfoState.createProperty('userInfo');
adapter.use(new BotStateSet(conversationState, userInfoState));

// Define a dialog set with state object set to the conversation state.
const dialogs = new DialogSet(conversationState.createProperty('dialogState'));

// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {

        const dc = await dialogs.createContext(context);

        switch(context.activity.type){
            case "conversationUpdate":
                // When a user starts a conversation with the bot, greet them and start
                // the appropriate dialog in the set.
                if(context.activity.membersAdded[0].id != context.activity.recipient.id){
                    await dc.context.sendActivity("Welcome to our hotel!")
                    return await dc.begin('checkInPrompt');
                }
                
            break;
            case "message":
                // Continue the current dialog if one is currently active
                var turnResult = await dc.continue(); 
                var userInfo = await userInfoAccessor.get(dc.context, {});

                if(turnResult.result && turnResult.result.guestInfo){
                    userInfo.guestInfo = turnResult.result.guestInfo;
                    return await dc.begin('mainMenu'); // Start the main menu
                }

                // Default action
                if (!context.responded) {
                    // If guest info is undefined prompt the user to check in
                    if(!userInfo.guestInfo){
                        return await dc.begin('checkInPrompt');
                    }else{
                        return await dc.begin('mainMenu'); 
                    }           
                }
            break;
        }
        
    });
});

dialogs.add(new WaterfallDialog('mainMenu', [
    async function (dc, step) {
        var userInfo = await userInfoAccessor.get(dc.context);
        var msg = `Hi ${userInfo.guestInfo.name}, how can I help you?`;
        const menu = ["Reserve Table", "Wake Up"];
        await dc.context.sendActivity(MessageFactory.suggestedActions(menu, msg));
        return Dialog.EndOfTurn;
    },
    async function (dc, step){        
        // Decide which module to start
        switch(step.result){
            case "Reserve Table":
                return await dc.begin('reservePrompt');
                break;
            case "Wake Up":
                return await dc.begin('wakeUpPrompt');
                break;
            default:
                await dc.context.sendActivity("Sorry, i don't understand that command. Please choose an option from the list below.");
                return await dc.replace('mainMenu');
                break;
        }
    },
    async function (dc, step){
        const userInfo = await userInfoAccessor.get(dc.context, {});

        if(step.result.tableInfo){
            userInfo.tableInfo = step.result.tableInfo;
        }
        else if(step.result.wakeUpInfo){
            userInfo.wakeUpInfo = step.result.wakeUpInfo;
        }

        return await dc.replace('mainMenu'); // Show the menu again
    }

]));

// Importing the dialogs 
const checkIn = require("./checkIn");
dialogs.add(new checkIn.CheckIn('checkInPrompt'));

const reserve_table = require("./reserveTable");
dialogs.add(new reserve_table.ReserveTable('reservePrompt'));

const wake_up = require("./wakeUp");
dialogs.add(new wake_up.WakeUp('wakeUpPrompt'));