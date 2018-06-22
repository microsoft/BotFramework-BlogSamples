
/*-----------------------------------------------------------------------------

This bot demonstrates how to use dialogs with a LuisRecognizer to add LUIS support to a bot.
LUIS identifies intents and entities from user messages, or utterances.

Intents map utterances to functionality in your bot.
In this example, the intents provide the following mappings:
 * The Notes.Create intent maps to the CreateNote handler
 * The Notes.Delete intent maps to the DeleteNote handler
 * The Notes.ReadAloud intent maps to the ReadNote handler

-----------------------------------------------------------------------------*/
var restify = require('restify');
var builder = require('botbuilder');

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});


// Create chat connector for communicating with the Bot Framework Service
// See https://aka.ms/node-env-var for information on setting environment variables in launch.json if you are using VSCode
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

// Listen for messages from users 
server.post('/api/messages', connector.listen());

// <Constructor>
// Create your bot
var bot = new builder.UniversalBot(connector);
// </Constructor>


// <Recognizer>
// Add global LUIS recognizer to bot
var luisAppUrl = process.env.LUIS_APP_URL || 'https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/<YOUR_APP_ID>?subscription-key=<YOUR_KEY>';
var notesRecognizer = bot.recognizer(new builder.LuisRecognizer(luisAppUrl));
// </Recognizer>

// <IntentDialogs>
var noteIntentsDialog = new builder.IntentDialog({
         recognizers: [notesRecognizer]
});

bot.dialog('/', noteIntentsDialog);
// </IntentDialogs>

// <NoneHandler>
// Handle the None intent
// This default message handler is invoked if the user's utterance doesn't
// match any other intents defined in the LUIS app.
noteIntentsDialog.matches('None', [
    function (session, args) {
    session.send("Hi... I'm the note bot sample. I can create new notes, read saved notes to you and delete notes.");

   // If the object for storing notes in session.userData doesn't exist yet, initialize it
   if (!session.userData.notes) {
       session.userData.notes = {};
       console.log("initializing userData.notes in default message handler");
   }
}]);
// </NoneHandler>

// <CreateNoteHandler>
// Handle the Note.Create intent
noteIntentsDialog.matches('Note.Create', [
    function (session, args, next) {
        // Resolve and store any Note.Title entity passed from LUIS.
        var intent = args.intent;
        var title = builder.EntityRecognizer.findEntity(intent.entities, 'Note.Title');

        var note = session.dialogData.note = {
          title: title ? title.entity : null,
        };
        
        // Prompt for title
        if (!note.title) {
            builder.Prompts.text(session, 'What would you like to call your note?');
        } else {
            next();
        }
    },
    function (session, results, next) {
        var note = session.dialogData.note;
        if (results.response) {
            note.title = results.response;
        }

        // Prompt for the text of the note
        if (!note.text) {
            builder.Prompts.text(session, 'What would you like to say in your note?');
        } else {
            next();
        }
    },
    function (session, results) {
        var note = session.dialogData.note;
        if (results.response) {
            note.text = results.response;
        }
        
        // If the object for storing notes in session.userData doesn't exist yet, initialize it
        if (!session.userData.notes) {
            session.userData.notes = {};
            console.log("initializing session.userData.notes in CreateNote dialog");
        }
        // Save notes in the notes object
        session.userData.notes[note.title] = note;

        // Send confirmation to user
        session.endDialog('Creating note named "%s" with text "%s"',
            note.title, note.text);
    }
]);
// </CreateNoteHandler>

// <DeleteNoteHandler>
// Handle the Note.Delete intent
noteIntentsDialog.matches('Note.Delete', [
    function (session, args, next) {
        if (noteCount(session.userData.notes) > 0) {
            // Resolve and store any Note.Title entity passed from LUIS.
            var title;
            var intent = args.intent;
            var entity = builder.EntityRecognizer.findEntity(intent.entities, 'Note.Title');
            if (entity) {
                // Verify that the title is in our set of notes.
                title = builder.EntityRecognizer.findBestMatch(session.userData.notes, entity.entity);
            }
            
            // Prompt for note name
            if (!title) {
                builder.Prompts.choice(session, 'Which note would you like to delete?', session.userData.notes);
            } else {
                next({ response: title });
            }
        } else {
            session.endDialog("No notes to delete.");
        }
    },
    function (session, results) {
        delete session.userData.notes[results.response.entity];        
        session.endDialog("Deleted the '%s' note.", results.response.entity);
    }
]);
// </DeleteNoteHandler>

// <ReadNoteHandler>
// Handle the Notes.ReadAloud intent
noteIntentsDialog.matches('Note.ReadAloud', [
    function (session, args, next) {
        if (noteCount(session.userData.notes) > 0) {
           
            // Resolve and store any Note.Title entity passed from LUIS.
            var title;
            var intent = args.intent;
            var entity = builder.EntityRecognizer.findEntity(intent.entities, 'Note.Title');
            if (entity) {
                // Verify it's in our set of notes.
                title = builder.EntityRecognizer.findBestMatch(session.userData.notes, entity.entity);
            }
            
            // Prompt for note name
            if (!title) {
                builder.Prompts.choice(session, 'Which note would you like to read?', session.userData.notes);
            } else {
                next({ response: title });
            }
        } else {
            session.endDialog("No notes to read.");
        }
    },
    function (session, results) {        
        session.endDialog("Here's the '%s' note: '%s'.", results.response.entity, session.userData.notes[results.response.entity].text);
    }
]);
// </ReadNoteHandler>

// <CountNotesHelper>
// Helper function to count the number of notes stored in session.userData.notes
function noteCount(notes) {

    var i = 0;
    for (var name in notes) {
        i++;
    }
    return i;
}
// </CountNotesHelper>
