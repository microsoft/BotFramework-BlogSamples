/*
 * Botbuilder v4 SDK - QnAMaker bot.
 * 
 * This bot demonstrates how to use QnAMaker to answer user's questions.
 * For this sample to work, you will need to have a QnAMaker account set up with a defined / trained
 * Knowledge Base to reference.
 * You can create a FREE QnAMaker account here: http://qnamaker.ai. 
 * Help instructions can be found here:
 * https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-qna?view=azure-bot-service-4.0&tabs=js
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --savea botbuilder-ai@preview
 * 
 * 2) From VSCode, open the package.json file and
 * Update the property "main" to the name of the sample bot you want to run. 
 *    For example: "main": "qnamaker/app.js" to run the this sample bot.
 * 3) run the bot in debug mode. 
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" or ask the bot a question to engage with the bot.
 *
 */ 

// Packages are installed for you
const { BotFrameworkAdapter, MemoryStorage, ConversationState } = require('botbuilder');
const restify = require('restify');
const { QnAMaker } = require('botbuilder-ai');

// Connecting to the QnAMaker
const qna = new QnAMaker(
    {
        // knowledgeBaseId: '<KNOWLEDGE-BASE-ID>',
        // endpointKey: '<QNA-SUBSCRIPTION-KEY>',
        // host: 'https://westus.api.cognitive.microsoft.com/qnamaker/v2.0'
        knowledgeBaseId: 'ed954b2b-d3d4-439e-8d09-dd900e1ea1cd',
        endpointKey: 'cf5d25e4-e1f0-4e63-96a2-67cb8212b086',
        host: 'https://cash-qnamaker1.azurewebsites.net/qnamaker'
    },
    {
        // set this to true to send answers from QnA Maker
        answerBeforeNext: true
    }
);


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
adapter.use(conversationState);

// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    adapter.processActivity(req, res, async (context) => {
        const isMessage = (context.activity.type === 'message');
        
        if (isMessage) {
            var handled = await qna.answer(context)
            if (!handled) {
                await context.sendActivity(`I'm sorry. I didn't understand.`);
            }

        } else if (context.activity.type !== 'message') {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }

    });
});

