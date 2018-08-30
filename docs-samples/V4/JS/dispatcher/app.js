/*
 * Botbuilder v4 SDK - Getting Started sample bot.
 * 
 * This is the JS sample code of the EchoBot. 
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * 
 * 2) From VSCode, run the bot in debug mode. 
 * 3) Load the emulator and point it to: http://localhost:3978/api/messages
 * 4) Send the bot a message. The bot will echo back what you send with "#: You said ..."
 * 
 * 2) From VSCode, open the package.json file and make sure that "main" is not set to any path (or is undefined) 
 * 3) Navigate to your bot app.js file and run the bot in debug mode (eg: click Debug/Start debuging)
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

// Packages are installed for you
const restify = require('restify');
const { BotFrameworkAdapter, BotStateSet, MiddlewareSet, MemoryStorage, ConversationState } = require('botbuilder');
const { LuisRecognizer, QnAMaker } = require('botbuilder-ai');
const { DialogSet, WaterfallDialog } = require('botbuilder-dialogs');

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

// Add conversation state middleware
const conversationState = new ConversationState(new MemoryStorage());
const homeAutomationAccessor = conversationState.createProperty('homeAutomation'); // Data bag for Home Automation data
adapter.use(new BotStateSet(conversationState));

// register some dialogs for usage with the LUIS apps that are being dispatched to
const dialogs = new DialogSet(conversationState.createProperty('dialogState'));

// Create LuisRecognizers and QnAMaker
// The LUIS applications are public, meaning you can use your own subscription key to test the applications.
// For QnAMaker, users are required to create their own knowledge base.
// The exported LUIS applications and QnAMaker knowledge base can be found with the sample bot.

// The corresponding LUIS application JSON is `dispatchSample.json`
const luisDispatcher = new LuisRecognizer({
    // This applicationId is for a public app that's made available for demo purposes
    // You can use it or use your own LUIS "Application ID" at https://www.luis.ai under "App Settings".
    applicationId: '0b18ab4f-5c3d-4724-8b0b-191015b48ea9',      // LUIS application ID
    // Replace endpointKey with your "Subscription Key"
    // your key is at https://www.luis.ai under Publish > Resources and Keys, look in the Endpoint column
    // The "subscription-key" is embeded in the Endpoint link. 
    endpointKey: 'f2eba92fcc6a4957a77134ebc9b437fc',            // LUIS subscription key
    //You can find your app's region info embeded in the Endpoint link as well.
    // Some examples of regions are `westus`, `westcentralus`, `eastus2`, and `southeastasia`.
    azureRegion: 'westus', 
    verbose: true
});

// adapter.use(dispatcher); // LuisRecognizer as middleware is no longer supported

// The corresponding LUIS application JSON is `homeautomation.json`
const homeAutomation = new LuisRecognizer({
    applicationId: '5815e389-0dbf-4b3e-a0f7-00eb9e2f4d19',
    endpointKey: 'f2eba92fcc6a4957a77134ebc9b437fc',
    azureRegion: 'westus', //'https://westus.api.cognitive.microsoft.com/',
    verbose: true
});

// The corresponding LUIS application JSON is `weather.json`
const weather = new LuisRecognizer({
    applicationId: 'f698134d-b8a9-45c3-87b8-0064165b4b66',
    endpointKey: 'f2eba92fcc6a4957a77134ebc9b437fc',
    azureRegion: 'westus', //'https://westus.api.cognitive.microsoft.com/',
    verbose: true
});

const faq = new QnAMaker(
    {
        knowledgeBaseId: 'b0c409e3-0b65-4b00-b716-62dab9a55dc6',
        endpointKey: '2397218d-e4d3-459d-87ea-e3bed984f5fb',
        host: 'https://cash-qna-dispatcher1.azurewebsites.net/qnamaker'
    },
    {
        answerBeforeNext: true
    }
);


function findEntities(entityName, entityResults) {
    let entities = []
    if (entityName in entityResults) {
        entityResults[entityName].forEach((entity, idx) => {
            entities.push(entity);
        });
    }
    return entities.length > 0 ? entities : undefined;
}

dialogs.add(new WaterfallDialog('HomeAutomation_TurnOff', [
    async (dc, step) => {
        var entities = step.options.entities;
        const devices = findEntities('HomeAutomation_Device', entities);
        const operations = findEntities('HomeAutomation_Operation', entities);

        const state = await homeAutomationAccessor.get(dc.context, {});
        state.homeAutomationTurnOff = state.homeAutomationTurnOff ? state.homeAutomationTurnOff + 1 : 1;
        await dc.context.sendActivity(`${state.homeAutomationTurnOff}: You reached the "HomeAutomation.TurnOff" dialog.`);
        if (devices) {
            await dc.context.sendActivity(`Found these "HomeAutomation_Device" entities:\n${devices.join(', ')}`);
        }
        if (operations) {
            await dc.context.sendActivity(`Found these "HomeAutomation_Operation" entities:\n${operations.join(', ')}`);
        }
        return await dc.end();
    }
]));

dialogs.add(new WaterfallDialog('HomeAutomation_TurnOn', [
    async (dc, step) => {
        var entities = step.options.entities;
        const devices = findEntities('HomeAutomation_Device', entities);
        const operations = findEntities('HomeAutomation_Operation', entities);

        const state = await homeAutomationAccessor.get(dc.context, {});
        state.homeAutomationTurnOn = state.homeAutomationTurnOn ? state.homeAutomationTurnOn + 1 : 1;
        await dc.context.sendActivity(`${state.homeAutomationTurnOn}: You reached the "HomeAutomation.TurnOn" dialog.`);
        if (devices) {
            await dc.context.sendActivity(`Found these "HomeAutomation_Device" entities:\n${devices.join(', ')}`);
        }
        if (operations) {
            await dc.context.sendActivity(`Found these "HomeAutomation_Operation" entities:\n${operations.join(', ')}`);
        }
        return await dc.end();
    }
]));

dialogs.add(new WaterfallDialog('Weather_GetForecast', [
    async (dc, step) => {
        var entities = step.options.entities;
        const locations = findEntities('Weather_Location', entities);

        const state = await homeAutomationAccessor.get(dc.context, {});
        state.weatherGetForecast = state.weatherGetForecast ? state.weatherGetForecast + 1 : 1;
        await dc.context.sendActivity(`${state.weatherGetForecast}: You reached the "Weather.GetForecast" dialog.`);
        if (locations) {
            await dc.context.sendActivity(`Found these "Weather_Location" entities:\n${locations.join(', ')}`);
        }
        return await dc.end();
    }
]));

dialogs.add(new WaterfallDialog('Weather_GetCondition', [
    async (dc, step) => {
        var entities = step.options.entities;
        const locations = findEntities('Weather_Location', entities);

        const state = await homeAutomationAccessor.get(dc.context, {});
        state.weatherGetCondition = state.weatherGetCondition ? state.weatherGetCondition + 1 : 1;
        await dc.context.sendActivity(`${state.weatherGetCondition}: You reached the "Weather.GetCondition" dialog.`);
        if (locations) {
            await dc.context.sendActivity(`Found these "Weather_Location" entities:\n${locations.join(', ')}`);
        }
        return await dc.end();
    }
]));

dialogs.add(new WaterfallDialog('None', [
    async (dc, step) => {
        const state = await homeAutomationAccessor.get(dc.context, {});
        state.noneIntent = state.noneIntent ? state.noneIntent + 1 : 1;
        await dc.context.sendActivity(`${state.noneIntent}: You reached the "None" dialog.`);
        return await dc.end();
    }
]));

// Listen for incoming Activities
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = context.activity.type === 'message';
        if (isMessage) {
            //const state = conversationState.get(context);
            const dc = await dialogs.createContext(context);

            // Retrieve the LUIS results from our dispatcher LUIS application
            const luisResults = await luisDispatcher.recognize(context);

            // Extract the top intent from LUIS and use it to select which LUIS application to dispatch to
            const topIntent = LuisRecognizer.topIntent(luisResults);

            switch (topIntent) {
                case 'l_homeautomation':
                    const homeAutoResults = await homeAutomation.recognize(context);
                    const topHomeAutoIntent = LuisRecognizer.topIntent(homeAutoResults);
                    return await dc.begin(topHomeAutoIntent, homeAutoResults);
                    break;
                case 'l_weather':
                    const weatherResults = await weather.recognize(context);
                    const topWeatherIntent = LuisRecognizer.topIntent(weatherResults);
                    return await dc.begin(topWeatherIntent, weatherResults);
                    break;
                case 'q_faq':
                    var answered = await faq.answer(context);
                    if(!answered){
                        // Handle fallthrough case
                        await dc.context.sendActivity(`Sorry, I don't understand, please try again.`);
                    }
                    break;
                default:
                    return await dc.begin('None');
            }

            if (!context.responded) {
                await dc.continue();

                // Default message
                if (!context.responded && isMessage) {
                    await dc.context.sendActivity(`Hi! I'm the LUIS dispatch bot. Say something and LUIS will decide how the message should be routed.`);
                }
            }
        }
    });
});
