/*
 * Botbuilder v4 SDK - Complex Conversation Flows.
 * 
 * This bot demonstrates how to use dialogs, waterfall, and prompts to manage complex conversation flows with loops.
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
const { DialogSet, WaterfallDialog, TextPrompt, DateTimePrompt, NumberPrompt, ChoicePrompt } = require('botbuilder-dialogs');

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
const reservationInfoState = conversationState.createProperty("reserverationInfo");
const userInfoAccessor = conversationState.createProperty('userInfo');
adapter.use(new BotStateSet(conversationState, userState));

const dialogs = new DialogSet(conversationState.createProperty('dialogState'));

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        // State will store all of your information 
        // const convo = conversationState.get(context);
        const dc = await dialogs.createContext(context);
        
        if (isMessage) {

            // Check for valid intents
            if(context.activity.text.match(/hello/ig)){
                return await dc.begin('greetings');
            }
            else if(context.activity.text.match(/menu/ig)){
                return await dc.begin('mainMenu');
            }
        }

        if(!context.responded){
            // Continue executing the "current" dialog, if any.
            var results = await dc.continue();

            if(!context.responded && isMessage){
                // Default message
                await context.sendActivity("Hi! I'm a simple bot. Please say 'Hello' or 'Menu'.");
            }
        }
    });
});


// Greet user:
// Ask for the user name and then greet them by name.
// Ask them where they work.
dialogs.add(new WaterfallDialog('greetings', [
    async function (dc, step){
        step.values.userInfo = {}; // New object
        return await dc.prompt('textPrompt', 'Hi! What is your name?');
    },
    async function(dc, step){
        var userName = step.result;
        step.values.userInfo.userName = userName;
        await dc.context.sendActivity(`Hi ${userName}!`);
        return await dc.prompt('textPrompt', 'Where do you work?');
    },
    async function(dc, step){
        var workPlace = step.result;
        step.values.userInfo.workPlace = workPlace;
        await dc.context.sendActivity(`${workPlace} is a fun place.`);

        // Persist user data
        const userData = await userInfoAccessor.get(dc.context, {});
        userData.userInfo = step.values.userInfo;

        return await dc.end(); // Ends the dialog
    }
]));

// Display a menu and ask user to choose a menu item. Direct user to the item selected.
dialogs.add(new WaterfallDialog('mainMenu', [
    async function(dc, step){
        await dc.context.sendActivity("Welcome to Contoso Hotel and Resort.");
        return await dc.prompt('choicePrompt', "How may we serve you today?", ['Order Dinner', 'Reserve a table']);
    },
    async function(dc, step){
        if(step.result.value.match(/order dinner/ig)){
            return await dc.begin('orderDinner');
        }
        else if(step.result.value.match(/reserve a table/ig)){
            return await dc.begin('reserveTable');
        }
    },
    async function(dc, step){
        // Start over
        await dc.cancelAll();
        return await dc.begin('mainMenu');
    }
]));

// Order dinner:
// Help user order dinner from a menu

const dinnerMenu = {
    choices: ["Potato Salad - $5.99", "Tuna Sandwich - $6.89", "Clam Chowder - $4.50", 
        "Process order", "Cancel"],
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
    async function (dc, step){
        await dc.context.sendActivity("Welcome to our Dinner order service.");
        
        return await dc.begin('orderPrompt', step.values.orderCart); // Prompt for orders
    },
    async function (dc, step) {
        if(step.result == "Cancel"){
            return await dc.end();
        }
        else { 
            return await dc.prompt('numberPrompt', "What is your room number?");
        }
    },
    async function(dc, step){
        await dc.context.sendActivity(`Thank you. Your order will be delivered to room ${step.result} within 45 minutes.`);
        return await dc.end();
    }
]));

// Helper dialog to repeatedly prompt user for orders
dialogs.add(new WaterfallDialog('orderPrompt', [
    async function(dc, step){
        var orderCart = (step.options.orders ? step.options : step.result); // If no data is passed in, step.result is undefined
        // Define a new cart if one does not exists
        if(!orderCart){
            // Initialize a new cart
            // convoState = conversationState.get(dc.context);
            step.values.orderCart = {
                orders: [],
                total: 0
            };
        }
        else {
            step.values.orderCart = orderCart;
        }
        return await dc.prompt('choicePrompt', "What would you like?", dinnerMenu.choices);
    },
    async function(dc, step){
        // Get state object
        // convoState = conversationState.get(dc.context);
        var choice = step.result;
        if(choice.value.match(/process order/ig)){
            if(step.values.orderCart.orders.length > 0) {
                // Process the order
                // ...
                step.values.orderCart = undefined; // Reset cart
                await dc.context.sendActivity("Processing your order.");
                return await dc.end();
            }
            else {
                await dc.context.sendActivity("Your cart was empty. Please add at least one item to the cart.");
                // Ask again
                return await dc.replace('orderPrompt');
            }
        }
        else if(choice.value.match(/cancel/ig)){
            //dc.activeDialog.state.orderCart = undefined; // Reset cart
            await dc.context.sendActivity("Your order has been canceled.");
            return await dc.end(choice.value);
        }
        else {
            var item = dinnerMenu[choice.value];

            // Only proceed if user chooses an item from the menu
            if(!item){
                await dc.context.sendActivity("Sorry, that is not a valid item. Please pick one from the menu.");
                
                // Ask again
                return await dc.replace('orderPrompt');
            }
            else {
                // Add the item to cart
                step.values.orderCart.orders.push(item);
                step.values.orderCart.total += item.Price;

                await dc.context.sendActivity(`Added to cart: ${choice.value}. <br/>Current total: $${step.values.orderCart.total}`);

                // Ask again
                return await dc.replace('orderPrompt', step.values.orderCart); // passing data into the replacing dialog
            }
        }
    }
]));

// Reserve a table:
// Help the user to reserve a table

dialogs.add(new WaterfallDialog('reserveTable', [
    async function(dc, step){
        await dc.context.sendActivity("Welcome to the reservation service.");

        myStateData = await reservationInfoState.get(dc.context, "myStateData2");

        step.values.reservationInfo = {}; // Clears any previous data
        return await dc.prompt('dateTimePrompt', "Please provide a reservation date and time.");
    },
    async function(dc, step){
        step.values.reservationInfo.dateTime = step.result[0].value;

        // Ask for next info
        return await dc.prompt('partySizePrompt', "How many people are in your party?");
    },
    async function(dc, step){
        step.values.reservationInfo.partySize = step.result;

        // Ask for next info
        return await dc.prompt('textPrompt', "Who's name will this be under?");
    },
    async function(dc, step){
        step.values.reservationInfo.reserveName = step.result;
        
        // Persist data
        const reservationState = await reservationInfoState.get(dc.context, {});
        reservationState.reservationInfo = step.values.reservationInfo;

        // Confirm reservation
        var msg = `Reservation confirmed. Reservation details: 
            <br/>Date/Time: ${step.values.reservationInfo.dateTime} 
            <br/>Party size: ${step.values.reservationInfo.partySize} 
            <br/>Reservation name: ${step.values.reservationInfo.reserveName}`;
            
        await dc.context.sendActivity(msg);
        return await dc.end();
    }
]));

// Define prompts
// Generic prompts
dialogs.add(new TextPrompt('textPrompt'));
dialogs.add(new DateTimePrompt('dateTimePrompt'));
dialogs.add(new NumberPrompt('numberPrompt'));
dialogs.add(new ChoicePrompt('choicePrompt'));
dialogs.add(new NumberPrompt('partySizePrompt', partySizeValidator));

async function partySizeValidator(context, promptValidatorContext){
    var msg = null;
    if(promptValidatorContext.recognized.value < 5) {
        msg = `Party size too small.`;
    }
    else if(promptValidatorContext.recognized.value > 50){
        msg = `Party size too big.`;
    }

    // If error, send message and repompt message
    //  - if a message is sent, you must also include reprompt message
    //    otherwise, it will attempt to send from the prompt or the retry message (if one is defined)
    // Else, end prompt so that waterfall can move to next step
    
    if(msg){ // fail
        msg += " Please enter a number between 5 and 50."
        await context.sendActivity(msg);
    }
    else{ // pass
        promptValidatorContext.end(promptValidatorContext.recognized.value);
    }
    
}