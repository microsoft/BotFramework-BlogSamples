/*
 * Botbuilder V4 SDK - Multi Conversation Flow using Topic Flags
 * No Botbuilder dialogs or prompts abstraction libraries. 
 * However, this bot will use a text recognizer library to parse for dates.
 * More info about this library here: 
 * https://github.com/Microsoft/Recognizers-Text/tree/master/JavaScript/packages/recognizers-text-suite 
 * 
 * This sample demonstrates how to manually manage a several conversations in one bot.
 * The conversations are managed one at a time.
 * Information will be stored to the "ConversationState" storage bag.
 * 
 * This bot can handle three conversations:
 * 1) Manage the user's profile and greet user by name if profile info exists. This is an automatic/default conversation,
 *    it is only triggers if the bot does not have a profile defined for the user.
 * 2) Reserve a table
 *      - also shows how to validate dateTime and partySize
 * 3) Order dinner
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --save @microsoft/recognizers-text-suite
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
const storage = new MemoryStorage(); // Volatile memory
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

            // Define a topicStates object if it doesn't exist in the convo state.
            if(!convo.topicStates){
                convo.topicStates = { // Define a default state object. Once done, reset back to undefined.
                    "topicTitle": undefined,
                    "prompt": undefined
                }
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
                // Continue profileTopic conversation
                await gatherUserProfile(context, convo);
            }
            else if(convo.topicStates.topicTitle == "reserveTable"){
                await reserveTable(context, convo);
            }
            else if(convo.topicStates.topicTitle == "orderDinner"){
                await orderDinner(context, convo);
            }

            // Check for valid intents
            else if(context.activity.text){
                if(context.activity.text.match(/hi/ig)){
                    await context.sendActivity(`Hi ${convo.userProfile.userName}.`);
                }
                else if(context.activity.text.match(/reserve table/ig)){
                    convo.topicStates.topicTitle = "reserveTable";
                    convo.reservationInfo = {}; // Define the storage bag

                    
                    await context.sendActivity(`Welcome to the Table Reservation service.`);
                    await reserveTable(context, convo);
                }
                else if(context.activity.text.match(/order dinner/ig)){
                    convo.topicStates.topicTitle = "orderDinner";
                    convo.topicStates.prompt = "orderPrompt";

                    // Initialize a new cart
                    convo.orderCart = {
                        orders: [],
                        total: 0
                    };

                    await context.sendActivity(`Welcome to the Dinner Ordering service.`);
                    await orderDinner(context, convo);
                }
                else {
                    // Invalid choice
                    await context.sendActivity(`Sorry, I don't understand that. Please choose from the menu.`);
                }
            }
            
            // If we are not in a conversation then show the main menu
            if(!convo.topicStates){
                // Default menu
                await showMenu(context);
            }
        }

    });
});

/////////////////////////////////
// Define the conversation flows

async function showMenu(context){
    await context.sendActivity(MessageFactory.suggestedActions(["Reserve table", "Order Dinner"], "How may we serve you today?"));
}

// User profile
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
        convo.topicStates.prompt = "room";
        await context.sendActivity("What room are you staying in?");
    }
    else if(convo.topicStates.prompt == "room"){
        // Save user's response
        convo.userProfile.room = context.activity.text;

        // Done
        convo.topicStates = undefined; // Reset object
        
        await context.sendActivity("Thank you. Your profile is complete.");
    }

}

// Reserve table
// Allow the user to reserve a table

async function reserveTable(context, convo){

    if(!convo.reservationInfo.dateTime && !convo.topicStates.prompt){
        convo.topicStates.prompt = "dateTime";
        await context.sendActivity("Please provide a reservation date and time.");
    }
    else if(convo.topicStates.prompt == "dateTime"){
        var dateTime = await Recognizers.recognizeDateTime(context.activity.text, Recognizers.Culture.English);
        if(await validateDateTime(context, dateTime[0].resolution.values)){
            // Save user's response
            convo.reservationInfo.dateTime = dateTime[0].resolution.values[0].value;

            // Ask next question
            convo.topicStates.prompt = "partySize";
            await context.sendActivity("How many people are in your party?");
        }
        else {
            // Ask again
            await context.sendActivity("Please provide a reservation date and time (e.g.: tomorrow at 3pm).");
        }
    }
    else if(convo.topicStates.prompt == "partySize"){
        if(await validatePartySize(context, context.activity.text)){
            // Save user's response
            convo.reservationInfo.partySize = context.activity.text;
            
            // Ask next question
            convo.topicStates.prompt = "reserveName";
            await context.sendActivity("Who's name will this be under?");
        }
        else {
            // Ask again
            await context.sendActivity("How many people are in your party?");
        }

    }
    else if(convo.topicStates.prompt == "reserveName"){
        // Save user's response
        convo.reservationInfo.reserveName = context.activity.text;

        // Done
        convo.topicStates = undefined;  // Reset object

        // Confirm reservation
        var msg = `Reservation confirmed. Reservation details: 
            <br/>Date/Time: ${convo.reservationInfo.dateTime} 
            <br/>Party size: ${convo.reservationInfo.partySize} 
            <br/>Reservation name: ${convo.reservationInfo.reserveName}`;
        await context.sendActivity(msg);
    }
    
}

// valideDateTime
// Check for a valid date/time in the future
async function validateDateTime(context, values){
    try {
        if (values.length < 0) { throw new Error('Missing time') }
        if (values[0].type !== 'datetime') { throw new Error('Unsupported type') }
        const value = new Date(values[0].value);
        if (value.getTime() < new Date().getTime()) { throw new Error('In the past') }
        return true;
    } catch (err) {
        await context.sendActivity(`${err.message} <br/>Please enter a valid time in the future like "tomorrow at 9am".`);
        return false;
    }
}

// validatePartySzie
// Support party size between 6 and 20 only
async function validatePartySize(context, value){
    try {
        if(value < 6) {
            throw new Error(`Party size too small.`);
        }
        else if(value > 20){
            throw new Error(`Party size too big.`);
        }
        return true; // Return the valid value
    }
    catch (err){
        await context.sendActivity(`${err.message} <br/>Please specify a number between 6 - 20.`);
        return false;
    }
}

// Order dinner
// Allow the user to order dinner to be delivered to their room

// Help user order dinner from a menu

var dinnerMenu = {
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

// This module allows user to add multiple items to a cart.
// It also supports actions: Process order and Cancel
async function orderDinner(context, convo){
    var choice = context.activity.text;

    // Check for action
    if(choice.match(/process order/ig)){
        if(convo.orderCart.orders.length > 0) {
            // Process the order
            // ...
            convo.orderCart = undefined;        // Reset cart
            convo.topicStates = undefined;      // Reset object
            await context.sendActivity("Processing your order.");
        }
        else {
            await context.sendActivity("Your cart was empty. Please add at least one item to the cart.");
            // Ask again
            await askForOrder(context);
        }
        return;
    }
    else if(choice.match(/cancel/ig)){
        convo.orderCart = undefined;        // Reset cart
        convo.topicStates = undefined;      // Reset to object
        await context.sendActivity(`Your order has been canceled`);
        return;
    }
    
    // Add item to cart
    else if(convo.topicStates.prompt == "addItem"){
        var item = dinnerMenu[choice];

        // Only proceed if user chooses an item from the menu
        if(!item){
            await context.sendActivity("Sorry, that is not a valid item. Please pick one from the menu.");
        }
        else {
            // Add the item to cart
            convo.orderCart.orders.push(item);
            convo.orderCart.total += item.Price;

            await context.sendActivity(`Added to cart: ${choice}. <br/>Current total: $${convo.orderCart.total}`);
        }
    }

    // Show the order menu
    await askForOrder(context, convo);
}

// Prompt for order
async function askForOrder(context, convo){
    convo.topicStates.prompt = "addItem";
    await context.sendActivity(MessageFactory.suggestedActions(dinnerMenu.choices, "What would you like?"));
}