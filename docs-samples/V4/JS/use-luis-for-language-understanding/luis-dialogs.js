/*
 * Botbuilder v4 SDK - Using LUIS for language understanding
 * 
 * This bot demonstrates how to use LUIS with dialogs to trigger multi-turn conversation flow
 * This example bot works with a LUIS app that detects intents used to trigger either a home automation
 * dialog or a weather dialog.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --save botbuilder-ai@preview
 * 
 * 2) From VSCode, open the package.json file and
 * Update the property "main" to the name of the sample bot you want to run. 
 *    For example: "main": "use-luis-for-language-understanding/luis-dialogs.js" to run the this sample bot.
 * 3) run the bot in debug mode. 
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "turn on the lights weather" to engage with the bot.
 *
 */ 


const { BotFrameworkAdapter, ConversationState, MemoryStorage, TurnContext } = require('botbuilder');
const { LuisRecognizer } = require('botbuilder-ai');
const { DialogSet } = require('botbuilder-dialogs');
const restify = require('restify');

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

// Create LuisRecognizer 
// The LUIS application is public, meaning you can use your own subscription key to test the applications.
const luisRecognizer = new LuisRecognizer({
    appId: '1fefd4a7-ae5b-4e63-99f7-0cf499a1423b',
    subscriptionKey: '<your subscription key>',
    serviceEndpoint: 'https://westus.api.cognitive.microsoft.com/',
    verbose: true
});

// Add the recognizer to your bot
adapter.use(luisRecognizer);

// create conversation state
const conversationState = new ConversationState(new MemoryStorage());
adapter.use(conversationState);

// register some dialogs for usage with the intents detected by the LUIS app
const dialogs = new DialogSet();

dialogs.add('HomeAutomation_TurnOn', [
    async (dialogContext) => {
        const state = conversationState.get(dialogContext.context);
        // state.homeAutomationTurnOn counts how many times this dialog was called 
        state.homeAutomationTurnOn = state.homeAutomationTurnOn ? state.homeAutomationTurnOn + 1 : 1;
        await dialogContext.context.sendActivity(`${state.homeAutomationTurnOn}: You reached the "HomeAutomation_TurnOn" dialog.`);

        await dialogContext.end();
    }
]);

// Pass the LUIS recognizer result to the args parameter
dialogs.add('Weather_GetForecast', [
    async (dialogContext, args) => {
        const locations = findEntities('Weather_Location', args.entities);

        const state = conversationState.get(dialogContext.context);
        state.weatherGetForecast = state.weatherGetForecast ? state.weatherGetForecast + 1 : 1;
        await dialogContext.context.sendActivity(`${state.weatherGetForecast}: You reached the "Weather.GetForecast" dialog.`);
        if (locations) {
            await dialogContext.context.sendActivity(`Found these "Weather_Location" entities:\n${locations.join(', ')}`);
        }
        await dialogContext.end();
    }
]);

dialogs.add('None', [
    async (dialogContext) => {
        const state = conversationState.get(dialogContext.context);
        // state.None counts how many times this dialog was called        
        state.None = state.None ? state.None + 1 : 1;
        await dialogContext.context.sendActivity(`${state.None}: You reached the "None" dialog.`);

        await dialogContext.end();
    }
]);

server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        if (context.activity.type === 'message') {
            const state = conversationState.get(context);
            const dc = dialogs.createContext(context, state);

            // Retrieve the LUIS results from our LUIS application
            const luisResults = luisRecognizer.get(context);

            // Extract the top intent from LUIS and use it to select which dialog to start
            // "NotFound" is the intent name for when no top intent can be found.
            const topIntent = LuisRecognizer.topIntent(luisResults, "NotFound");

            const isMessage = context.activity.type === 'message';
            if (isMessage) {
                switch (topIntent) {
                    case 'homeautomation':                    
                        await dc.begin("HomeAutomation_TurnOn", luisResults);
                        break;
                    case 'weather':                    
                        await dc.begin("Weather_GetForecast", luisResults);
                        break;
                    case 'None':
                        await dc.begin("None");
                        break;
                    case 'NotFound':
                        await context.sendActivity(`Sorry, I didn't get any results from LUIS.`);
                        break;
                    default:
                        // handle intents for which we have no dialog
                        await context.sendActivity(`You reached the ${topIntent} intent.`);
                        break;
                }
            }
            
            if (!context.responded) {
                await dc.continue();
                if (!context.responded && isMessage) {
                    await dc.context.sendActivity(`Hi! I'm the LUIS dialog bot. Say something and LUIS will decide how the message should be routed.`);
                }
            }
        }
    });
});

// Helper function for finding a specified entity
// entityResults are the results from LuisRecognizer.get(context)
function findEntities(entityName, entityResults) {
    let entities = []
    if (entityName in entityResults) {
        entityResults[entityName].forEach(entity => {
            entities.push(entity);
        });
    }
    return entities.length > 0 ? entities : undefined;
}