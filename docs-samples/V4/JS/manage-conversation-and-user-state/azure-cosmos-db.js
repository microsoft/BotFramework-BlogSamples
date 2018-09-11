/*
 * Botbuilder v4 SDK - Azure Cosmos DB
 * 
 * Connect your bot storage to Azure Cosmos DB.
 * 
 * This bot demonstrates how to manage a conversation state and user state with Azure Cosmos DB as storage.
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


const { BotFrameworkAdapter, ConversationState, BotStateSet, MemoryStorage } = require('botbuilder');
const { CosmosDbStorage, BlobStorage } = require('botbuilder-azure');
const restify = require('restify');

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
// Add CosmosDB 
const storage = new CosmosDbStorage({
    serviceEndpoint: "https://lucascosmos.documents.azure.com:443/", 
    authKey: "xCmY4dZqZACCVUoExxAbfa38MwPjShqbm2TTqNhzKnqlTCLSc8mTio1TeCbEu2dCCg8VmNAohRARuUMPmmrRPg==", 
    databaseId: "Tasks",
    collectionId: "Items"
});

// const storage = new CosmosDbStorage({
//     serviceEndpoint: process.env.ACTUAL_SERVICE_ENDPOINT, 
//     authKey: process.env.ACTUAL_AUTH_KEY, 
//     databaseId: process.env.DATABASE,
//     collectionId: process.env.COLLECTION
// });

const conversationState = new ConversationState(storage);
adapter.use(new BotStateSet(conversationState));

// Listen for incoming activity .
server.post('/api/messages', (req, res) => {
    // Route received activity to adapter for processing.
    adapter.processActivity(req, res, async (context) => {
        if (context.activity.type === 'message') {
            const state = conversationState.get(context);
            const count = state.count === undefined ? state.count = 0 : ++state.count;

            await logMessageText(storage, context);

            // Demonstrating "optimistic concurancies" using ETag
            readNote(storage, context);
            writeNote(storage, context);

            await context.sendActivity(`${count}: You said "${context.activity.text}"`);
        } else {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }
    });
});

async function logMessageText(storage, context) {
    let utterance = context.activity.text;
    try {
        // Read from the storage.
        let storeItems = await storage.read(["UtteranceLog"])
        // Get to log.
        var utteranceLog = storeItems["UtteranceLog"];

        // If no stored messages were found, create an empty list.
        if (!utteranceLog) {
            await context.sendActivity(`Need to create new utterance log`);
            utteranceLog = storeItems["UtteranceLog"] = { UtteranceList: [], "eTag": "*" }
        } 

        // Add current message to list
        utteranceLog.UtteranceList.push(utterance);

        // Show user current list of saved messages
        await context.sendActivity(`The list now is: \n ${utteranceLog.UtteranceList.join(", ")}`);

        // Save the new list to storage
        try {
            await storage.write(storeItems)
            await context.sendActivity('Successfully write to utterance log.');
        } catch (err) {
            await context.sendActivity(`Write to UtteranceLog fail: ${err}`);
        }
    } catch (err) {
        await context.sendActivity(`Read rejected. ${err}`);
    };
}

// These two functions will demonstrate how concurancy work with ETag
// Writing a "note" to storage, then read it in at a later time, update it's "Contents" and 
// write the note back to storage.
async function writeNote(storage, context) {
    var note = {
        "Name": "Shopping list",
        "Contents": "eggs",
        "ETag": "001"
    }

    var changes = [];
    changes.push(note); // Add note to the changes list

    try {
        // Write changes to storage
        await storage.write(changes);
        await context.sendActivity(`Write succeeded: write changes to storage.`)
    }
    catch(err) {
        await context.sendActivity(`Write error: ${err}`);
    }
}

async function readNote(storage, context){
    try{
        var storeItems = await storage.read(["note"]);
        var note = storeItems["note"];

        if(note){
            note.Contents += ", bread";
            var changes = [];
            changes.push(note);

            // Write note back out to storage
            try {
                // Write changes to storage
                await storage.write(changes);
                await context.sendActivity(`Write succeeded: write changes to storage.`)
            }
            catch(err) {
                await context.sendActivity(`Write error: ${err}`);
            }
        }
    }
    catch(err){
        await context.sendActivity(`Read error: ${err}`);
    }
}