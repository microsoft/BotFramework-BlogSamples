global.restify = require('restify');
global.builder = require('botbuilder');
global.lodash = require('lodash');
global.promise = require('bluebird');
global.request = require('request-promise');

// Misc.
global.attachments = require('./lib/attachments');
global.jokes = require('./data/jokes.json');

// Cognitive Service Clients.
const QnAClient = require('./lib/qnaclient');
const BingSearchClient = require('./lib/bingsearchclient');
const SentimentAnalyzerClient = require('./lib/sentimentanalyzerclient');
const DialogAnalyzerClient = require('./lib/dialoganalyzerclient');

// Environment variables
const BOTBUILDER_APP_ID = process.env.BOTBUILDER_APP_ID;
const BOTBUILDER_APP_PASSWORD = process.env.BOTBUILDER_APP_PASSWORD;
const LUIS_MODEL = process.env.LUIS_MODEL;
const KB_ID = process.env.KB_ID;
const QNA_KEY = process.env.QNA_KEY;
const QNA_URL = process.env.QNA_URL;
const BING_SEARCH_CONFIG = process.env.BING_SEARCH_CONFIG;
const BING_SEARCH_KEY = process.env.BING_SEARCH_KEY;
const TEXT_ANALYTICS_KEY = process.env.TEXT_ANALYTICS_KEY;
const TEXT_ANALYTICS_URL = process.env.TEXT_ANALYTICS_URL;
const DIALOG_ANALYZER_CLIENTID = process.env.DIALOG_ANALYZER_CLIENTID;
const DIALOG_ANALYZER_KEY = process.env.DIALOG_ANALYZER_KEY;
const DIALOG_ANALYZER_URL = process.env.DIALOG_ANALYZER_URL;

// Check to see if the environment has been set.
if (!(BOTBUILDER_APP_ID &&
    BOTBUILDER_APP_PASSWORD &&
    LUIS_MODEL &&
    KB_ID &&
    QNA_KEY &&
    QNA_URL &&
    BING_SEARCH_CONFIG &&
    BING_SEARCH_KEY &&
    TEXT_ANALYTICS_KEY &&
    TEXT_ANALYTICS_URL &&
    DIALOG_ANALYZER_CLIENTID &&
    DIALOG_ANALYZER_KEY &&
    DIALOG_ANALYZER_URL
)) {
    console.log(`Missing one of BOTBUILDER_APP_ID, BOTBUILDER_APP_PASSWORD, \
    LUIS_MODEL, KB_ID, QNA_KEY, QNA_URL, BING_SEARCH_CONFIG, BING_SEARCH_KEY, \
    TEXT_ANALYTICS_KEY, TEXT_ANALYTICS_URL, DIALOG_ANALYZER_CLIENTID, DIALOG_ANALYZER_KEY or DIALOG_ANALYZER_URL \
    in environment variables!`);
    process.exit(1);
}

// QnAClient allows simple question and answer style responses.
global.qnaClient = new QnAClient({
    knowledgeBaseId: KB_ID,
    subscriptionKey: QNA_KEY
});

// Search the web for results.
global.bingSearchClient = new BingSearchClient({
    bingSearchConfig: BING_SEARCH_CONFIG,
    bingSearchKey: BING_SEARCH_KEY
});

// Determine user mood from text
global.sentimentAnalyzerClient = new SentimentAnalyzerClient({
    key: TEXT_ANALYTICS_KEY
});

global.dialogAnalyzerClient = new DialogAnalyzerClient({
    clientId: DIALOG_ANALYZER_CLIENTID,
    key: DIALOG_ANALYZER_KEY,
    url: DIALOG_ANALYZER_URL
});

// Setup Restify Server
const server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, () => {
    console.log('%s listening to %s', server.name, server.url);
});

// Create chat connector for communicating with the Bot Framework Service
const connector = new builder.ChatConnector({
    appId: BOTBUILDER_APP_ID,
    appPassword: BOTBUILDER_APP_PASSWORD
});

// Serves a nice web site, that has the bot framework web chat component.
server.get('/', restify.plugins.serveStatic({
    'directory': `${__dirname}/static`,
    'default': 'index.html'
}));

// Listen for messages from users
server.post('/api/messages', connector.listen());

// Create a LUIS recognizer for our bot, to identify intents from
// user text.
const recognizer = new builder.LuisRecognizer(LUIS_MODEL);

// Create our bot to listen in on the chat connector.
global.bot = new builder.UniversalBot(connector, (session) => {
    session.beginDialog('sobot:search')
});

bot.recognizer(recognizer);

bot.use(builder.Middleware.sendTyping());

// Sends a nice greeting on a new message.
bot.on('conversationUpdate', (message) => {
    if (!message.membersAdded) {
        return;
    }

    message.membersAdded.forEach((identity) => {
        if (identity.id !== message.address.bot.id) {
            return;
        }

        bot.send(new builder.Message()
            .address(message.address)
            .text(`👋 Hello! I'm Stack Overflow's Resident Expert Bot 🤖 \
                and I'm here to help you find questions, answers, or to \
                just entertain you with a joke. Go ahead - ask me something!`
            ));
    });
});

// Promise for obtaining JWT Token (requested once)
global.obtainToken = promise.promisify(connector.getAccessToken.bind(connector));

global.bingSearchQuery = async (session, args) => {
    session.send("Hmm... Searching...");
    session.sendTyping();

    if (!args) {
        return;
    }

    if (!args.query) {
        return;
    }

    if (!args.fullTextQuery) {
        args.fullTextQuery = args.query;
    }

    // Start and wait for Bing Search results.
    let searchResults = await fetchBingSearchResults(args.query);

    // Process search results
    if (searchResults && searchResults.length > 0) {
        session.send("I found the following results from your question...");
        session.send(attachments.buildResultsMessageWithAttachments(session, searchResults));
        return session.endDialog("Feel free to ask me another question, or even ask for a joke!");
    } else {
        return session.endDialog('Sorry… couldnt find any results for your query! 🤐');
    }
}

global.filterforUsefulResults = (resultsArray) => {
    const resultsCount = resultsArray.length > 10 ? 10 : resultsArray.length;
    return lodash.slice(resultsArray, 0, resultsCount);
};

global.fetchBingSearchResults = async (query) => {
    var searchResults = [];

    await bingSearchClient.get({ searchText: query }, (err, res) => {
        if (err) {
            console.error('Error from callback:', err);
        } else if (res && res.webPages && res.webPages.value && res.webPages.value.length > 0) {
            for (var index = 0; index < res.webPages.value.length; index++) {
                var val = res.webPages.value[index];
                var result = {
                    title: val.name,
                    body_markdown: val.snippet,
                    link: val.url
                };
                searchResults.push(result);
            }
        }
    });

    return searchResults;
}

global.cleanQueryString = (query, removePunctuation) => {
    let retQuery = query.toLowerCase();
    if (removePunctuation) {
        retQuery = retQuery.replace(/[^\w\s]|_/g, "");
    }
    retQuery = retQuery.replace(/\s+/g, " ");
    return retQuery.trim();
}

// Dialogs
require('./dialogs/brain')();
require('./dialogs/joke')();
require('./dialogs/keywordPrompt')();
require('./dialogs/languages')();
require('./dialogs/menu')();
require('./dialogs/screenshot')();
require('./dialogs/search')();
require('./dialogs/smalltalk')();