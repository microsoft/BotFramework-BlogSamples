/*
 * Botbuilder v4 SDK - QnAMaker bot with fall through handling.
 * 
 * This bot demonstrates how to use QnAMaker to answer user's questions and how to handle the question
 * if QnAMaker does not have an answer.
 * 
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
 * 2) From VSCode, open the package.json file and make sure that "main" is not set to any path (or is undefined) 
 * 3) Navigate to your bot app.js file and run the bot in debug mode (eg: click Debug/Start debuging)
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

// Packages are installed for you
const { BotFrameworkAdapter, MemoryStorage, ConversationState, MessageFactory } = require('botbuilder');
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

            // Handle user actions
            var action = context.activity.text;
            if(action.match(/^call support/i)){
                await context.sendActivity("Please call support at 202-303-4455");
            }
            else if(action.match(/^email support/i)){
                await context.sendActivity("Please send emails to <support@contoso.com>");
            }
            else if(action.match(/^live chat support/i)){
                await context.sendActivity("Please wait, redirecting you to Live Chat");
                //
                // ...Logic to redirect to live chat site
                //
            }

            // Handle user's question
            else {
                var handled = await qna.answer(context)
                
                // If no answer is found, give user some optional actions to take
                if (!handled) {
                const actions = ["Call support", "Email support", "Live chat support"];
                await context.sendActivity(MessageFactory.suggestedActions(actions, "No answer found. Here are your options:"));
                }
            }

        } else if (context.activity.type !== 'message') {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }

    });
});

