/*
 * Botbuilder v4 SDK - Azure Blob Storage
 * 
 * Connect your bot storage to Azure Blob Storage.
 * 
 * This bot demonstrates how to manage a conversation state and user state with Azure Blob Storage as storage.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * npm install --save botbuilder-azure@preview
 * 
 * 2) From VSCode, open the package.json file and make sure that "main" is not set to any path (or is undefined) 
 * 3) Navigate to your bot app.js file and run the bot in debug mode (eg: click Debug/Start debuging)
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */

const restify = require('restify');
const { BotFrameworkAdapter, ConversationState, BotStateSet, MemoryStorage } = require('botbuilder');
const { BlobStorage, AzureBlobTranscriptStore } = require('botbuilder-azure');

// Create server.
let server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`${server.name} listening to ${server.url}`);
});

// Create adapter.
const adapter = new BotFrameworkAdapter({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

// Add memory storage.
// Add Blob Storage 
const storage = new BlobStorage({
    containerName: "cashblobstorage91018",
    storageAccountOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=cashblobstorage91018;AccountKey=E6346MZXAoIAbSB9B6xNwqwdYMtzgRLPIC8T+ZlpEc2VOTZvphRC3llxhOZCqX+++Genw8xAHIc3oXM5ADoYgw==;EndpointSuffix=core.windows.net"
    // storageAccessKey: "E6346MZXAoIAbSB9B6xNwqwdYMtzgRLPIC8T+ZlpEc2VOTZvphRC3llxhOZCqX+++Genw8xAHIc3oXM5ADoYgw=="
    //host?: string | Host
});
// Add AzureBlobTranscriptStore 
const myTranscript = new AzureBlobTranscriptStore({
    containerName: "cashtranscriptblob",
    storageAccountOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=cashtranscriptblob;AccountKey=E6346MZXAoIAbSB9B6xNwqwdYMtzgRLPIC8T+ZlpEc2VOTZvphRC3llxhOZCqX+++Genw8xAHIc3oXM5ADoYgw==;EndpointSuffix=core.windows.net"
    // storageAccessKey: "E6346MZXAoIAbSB9B6xNwqwdYMtzgRLPIC8T+ZlpEc2VOTZvphRC3llxhOZCqX+++Genw8xAHIc3oXM5ADoYgw=="
    //host?: string | Host
});

const conversationState = new ConversationState(storage);
const convoStateAccessor = conversationState.createProperty('convoState');
adapter.use(new BotStateSet(conversationState));

// Listen for incoming activity .
server.post('/api/messages', (req, res) => {
    // Route received activity to adapter for processing.
    adapter.processActivity(req, res, async (context) => {
        if (context.activity.type === 'message') {
            const convoState = await convoStateAccessor.get(context, {});
            const count = convoState.count === undefined ? convoState.count = 0 : ++convoState.count;

            await myTranscript.logActivity(context.activity); // Log an activity to transcript

            await listTranscript();

            await context.sendActivity(`${count}: You said "${context.activity.text}"`);
        } else {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }
    });
});

async function listTranscript(){
    var storedTranscript = [];

    var pageResult = {
        results: undefined,
        continuationToken: {}
    }
    
    while(pageResult.continuationToken){
        pagedResult.results = await myTranscript.listTranscripts("emulator", pagedResult.continuationToken?continuationToken:null);

        for(i in pageResult.results.items){
            storedTranscript.push(pageResult.results.items[i].id);
        }
    }
    
}

async function getUserConversation(context){
    
}