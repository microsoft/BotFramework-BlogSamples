var builder = require('botbuilder');
var azure = require('botbuilder-azure');
var restify = require('restify');

// **UNCOMMENT THE FOLLOWING LINE TO CONNECT TO THE BOT FRAMEWORK EMULATOR
//process.env.NODE_TLS_REJECT_UNAUTHORIZED = 0;

var documentDbOptions = {
    host: 'Your-Azure-DocumentDB-URI',
    masterKey: 'Your-Azure-Key',
    database: 'botdocdb',
    collection: 'botdata'
};

var docDbClient = new azure.DocumentDbClient(documentDbOptions);

var tableStorage = new azure.AzureBotStorage({ gzipData: false }, docDbClient);

var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: %s", session.message.text);
}).set('storage', tableStorage);

var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url);
});

server.post('/api/messages', connector.listen());
