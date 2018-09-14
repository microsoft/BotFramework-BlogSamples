/*
 * Botbuilder v4 SDK - Multiple Conversation Flows.
 * 
 * This bot demonstrates how to use dialogs, waterfall, and prompts to manage conversation flows.
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
const { BotFrameworkAdapter, AutoSaveStateMiddleware, MemoryStorage, ConversationState, UserState, BotStateSet } = require('botbuilder');
const { Dialog, DialogSet, WaterfallDialog, TextPrompt, NumberPrompt, ChoicePrompt } = require('botbuilder-dialogs');

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
adapter.use(new AutoSaveStateMiddleware(conversationState, userInfoState));

// Define a dialog set with state object set to the conversation state.
const dialogs = new DialogSet(conversationState.createProperty('dialogState'));

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        //const convoState = await conversationState.get(context);
        const dc = await dialogs.createContext(context);
        
        if (isMessage) {
        //     // Handle user interrupts
        //     if(context.activity.text.match(/help/ig)){
        //         var msg = "To interact with the bot, send it any of these messages: 'check in', 'reserve table', or 'order dinner'.";
        //         await context.sendActivity(msg);
        //         return Dialog.EndOfTurn; // Ends the turn
        //     }
        //     else if(context.activity.text.match(/open hours/ig)){
        //         var msg = "Hours of operations: M-F 5AM - 11PM. Sat 9AM - 10PM. Sunday closed."
        //         await context.sendActivity(msg);
        //         return Dialog.EndOfTurn;
        //     }
        
            // Check for valid intents
            if(context.activity.text.match(/check in/ig)){
                return await dc.begin('checkIn');
            }
            else if(context.activity.text.match(/reserve table/ig)){
                return await dc.begin('reserveTable');
            }
            else if(context.activity.text.match(/order dinner/ig)){
                return await dc.begin('orderDinner');
            }            

            if(!context.responded){
                // Continue executing the "current" dialog, if any.
                var results = await dc.continue();

                // The dialog is complete with data passed back.
                if(results.status == "complete" && results.result){
                    // Do something with `results.result`
                    const userInfo = await userInfoAccessor.get(step.context, {});

                    // Persist data in appropriate bags
                    if(results.result.guestInfo){
                        userInfo.guestInfo = results.result.guestInfo;
                    }
                    else if(results.result.tableInfo){
                        userInfo.tableInfo = results.result.tableInfo;
                    }
                    else if(results.result.orderCart){
                        userInfo.orderCart = results.result.orderCart;
                    }
                }

                if(!context.responded && isMessage){
                    // Default message
                    await context.sendActivity("Hi! I'm a simple bot. Please say 'check in', 'reserve table', or 'order dinner'.");
                }
            }
        }
    });
});


// Check in user:
dialogs.add(new WaterfallDialog('checkIn', [
    async function (step) {
        // Create a new local guestInfo databag
        step.values.guestInfo = {};
        return await step.prompt('textPrompt', "Welcome to the 'Check In' service. <br/>What is your name?");
    },
    async function (step){
        // Save the name 
        var name = step.result;
        step.values.guestInfo.name = name;
        return await step.prompt('numberPrompt', `Hi ${name}. What room will you be staying in?`);
    },
    async function (step){
        // Save the room number
        var room = step.result;
        step.values.guestInfo.room = room
        await step.context.sendActivity(`Great, room ${room} is ready for you. Enjoy your stay!`);

        // End the dialog and return the guest info
        return await step.end(step.values);
    }
]));

// Reserve table
// Help the user reserve a dinner table
dialogs.add(new WaterfallDialog('reserveTable', [
    async function (step) {
        // Create a new local tableInfo databag
        step.values.tableInfo = {};

        const prompt = `Which table would you like to reserve?`;
        const choices = ['1', '2', '3', '4', '5', '6'];
        return await step.prompt('choicePrompt', prompt, choices);
    },
    async function ( step) {
        // Create a new local tableInfo databag
        step.values.tableInfo.tableNumber = step.result.value;

        return await step.prompt('textPrompt', `What is the reservation name?`);
    },
    async function(step){
        step.values.tableInfo.reserveName = step.result;
        await step.context.sendActivity(`Got it! Table number ${step.values.tableInfo.tableNumber} is reserved for ${step.values.tableInfo.reserveName}.`);
        
        // End the dialog and return the table information
       return  await step.end(step.values);
    }
]));

// Order dinner
// Help a user order an item from the menu
// Order dinner:
// Help user order dinner from a menu

const dinnerMenu = {
    choices: ["Potato Salad - $5.99", "Tuna Sandwich - $6.89", "Clam Chowder - $4.50", 
            "Cancel"],
    "Potato Salad - $5.99": {
        Description: "Potato Salad",
        Price: 5.99
    },
    "Tuna Sandwich - $6.89": {
        Description: "Tuna Sandwich",
        Price: 6.89
    },
    "Clam Chowder - $4.50": {
        Description: "Clam Chowder",
        Price: 4.50
    }
}

// Order dinner:
// Help user order dinner from a menu
dialogs.add(new WaterfallDialog('orderDinner', [
    async function (step){
        await step.context.sendActivity("Welcome to our Dinner order service.");
        var orderCart = (step.options.orders ? step.options : step.result); // If no data is passed in, step.result is undefined
        // Define a new cart if one does not exists
        if(!orderCart){
            // Initialize a new cart
            step.values.orderCart = {
                orders: [],
                total: 0
            };
        }
        else {
            step.values.orderCart = orderCart;
        }
        return await step.prompt('choicePrompt', "What would you like?", dinnerMenu.choices);
    },
    async function(step){
        var choice = step.result;
        if(choice.value.match(/cancel/ig)){
            await step.context.sendActivity("Your order has been canceled.");
            return await step.end();
        }
        else {
            var item = dinnerMenu[choice.value];

            // Only proceed if user chooses an item from the menu
            if(!item){
                await step.context.sendActivity("Sorry, that is not a valid item. Please pick one from the menu.");
                
                // Ask again
                return await step.replace('orderDinner');
            }
            else {
                // Add the item to cart
                step.values.orderCart.orders.push(item);
                step.values.orderCart.total += item.Price;

                await step.context.sendActivity(`Added to cart: ${choice.value}. <br/>Current total: $${step.values.orderCart.total}`);

                return await step.prompt('numberPrompt', "What is your room number?");
            }
        }
    },
    async function(step){
        await step.context.sendActivity(`Thank you. Your order will be delivered to room ${step.result} within 45 minutes.`);
        return await step.end(step.values);
    }
]));


// Define prompts
// Generic prompts
dialogs.add(new TextPrompt('textPrompt'));
dialogs.add(new NumberPrompt('numberPrompt'));
dialogs.add(new ChoicePrompt('choicePrompt'));