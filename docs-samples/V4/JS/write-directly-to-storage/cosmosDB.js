// paste the following into app.js
const { BotFrameworkAdapter, ConversationState, BotStateSet } = require('botbuilder');
const { CosmosDbStorage } = require("botbuilder-azure");
const restify = require('restify');
require('dotenv').config()


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


//Add CosmosDB 
const storage = new CosmosDbStorage({
    serviceEndpoint: process.env.ACTUAL_SERVICE_ENDPOINT, 
    authKey: process.env.ACTUAL_AUTH_KEY, 
    databaseId: process.env.DATABASE,
    collectionId: process.env.COLLECTION
})

const conversationState = new ConversationState(storage);
adapter.use(new BotStateSet(conversationState));


// Listen for incoming activity 
server.post('/api/messages', (req, res) => {
    // Route received activity to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        if (context.activity.type === 'message') {
            const state = conversationState.get(context);
            const count = state.count === undefined ? state.count = 0 : ++state.count;

            await logMessageText(storage, context);

            if (context.activity.text.includes("create note")) {
                await createSampleNote(storage, context);
            } else if (context.activity.text.includes("update note")) {
                await updateSampleNote(storage, context);
            }

            await context.sendActivity(`${count}: You said "${context.activity.text}"`);
        } else {
            await context.sendActivity(`[${context.activity.type} event detected]`);
        }
    });
});

async function logMessageText(storage, context) {
    let utterance = context.activity.text;
    let storeItems = await storage.read(["UtteranceLog"])
    try {
        // check result
        var utteranceLog = storeItems["UtteranceLog"];

        if (typeof (utteranceLog) != 'undefined') {
            // log exists so we can write to it
            storeItems["UtteranceLog"].UtteranceList.push(utterance);

            try {
                await storage.write(storeItems)
                context.sendActivity('Successful write to utterance log.');
            } catch (err) {
                context.sendActivity(`Write failed of UtteranceLog: ${err}`);
            }

        } else {
            context.sendActivity(`need to create new utterance log`);
            storeItems["UtteranceLog"] = { UtteranceList: [`${utterance}`], "eTag": "*" }

            try {
                await storage.write(storeItems)
                context.sendActivity('Successful write to log.');
            } catch (err) {
                context.sendActivity(`Write failed: ${err}`);
            }
        }
    } catch (err) {
        context.sendActivity(`Read rejected. ${err}`);
    };
}

// Helper function for writing a sample note to a data store
async function createSampleNote(storage, context) {
    var myNoteData = {
        name: "Shopping List",
        contents: "eggs",
        // If any Note file is already stored, the eTag field
        // must be set to "*" in order to allow writing without first reading the stored eTag
        // otherwise you'll likely get an exception indicating an eTag conflict. 
        eTag: "*"
    }

    // Write the note data to the "Note" key
    var changes = {};
    changes["Note"] = myNoteData;
    // Creates a file named Note, if it doesn't already exist.
    // specifying eTag= "*" will overwrite any existing contents.
    // The act of writing to the file automatically updates the eTag property
    // The first time you write to Note, the eTag is changed from *, and file contents will become:
    //    {"name":"Shopping List","contents":"eggs","eTag":"1"}
    try {
        await storage.write(changes);
        await context.sendActivity('Successful created a note.');
    } catch (err) {
        await context.sendActivity(`Could not create note: ${err}`);
    }
}


async function updateSampleNote(storage, context) {
    try {
        var note = await storage.read(["Note"]);
        console.log(`note.eTag=${note["Note"].eTag}\n note=${JSON.stringify(note)}`);
        // update the note that we just read
        note["Note"].contents += ", bread";
        console.log(`Updated note=${JSON.stringify(note)}`);

        try {
            await storage.write(note); // Write the changes back to storage
            await context.sendActivity('Successfully updated to note.');
        } catch (err) {
            console.log(`Write failed: ${err}`);
        }
    }
    catch (err) {
        await context.sendActivity(`Unable to read the Note: ${err}`);
    }
}