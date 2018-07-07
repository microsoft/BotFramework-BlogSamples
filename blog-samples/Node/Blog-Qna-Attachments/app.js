var restify = require('restify');
var builder = require('botbuilder');
var builder_cognitiveservices = require('botbuilder-cognitiveservices');
var https = require('https');

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function(){
    console.log('%s listening to %s', server.name, server.url);
});

// Create chat connector instance
var connector = new builder.ChatConnector({
    appId: 'Your-Microsoft-App-ID', //process.env.MicrosoftAppId,
    appPassword: 'Your-Microsoft-App-Password', //    process.env.MicrosoftAppPassword,
});

// Listen for messages from users
server.post('/api/messages', connector.listen());

// Bot instance, pass in the connector to receive messages from the user
var bot = new builder.UniversalBot(connector);

var recognizer = new builder_cognitiveservices.QnAMakerRecognizer({
    knowledgeBaseId: 'Your-Qna-KnowledgeBase-ID', // process.env.QnAKnowledgebaseId, 
    authKey: 'Your-Qna-KnowledgeBase-Password', //process.env.QnAAuthKey}),
    endpointHostName: 'Your-Qna-KnowledgeBase-HostName'}); //process.env.QnAEndpointHostName});

var basicQnAMakerDialog = new builder_cognitiveservices.QnAMakerDialog({
recognizers: [recognizer],
defaultMessage: 'No match! Try changing the query terms!',
qnaThreshold: 0.3}
);

// override
basicQnAMakerDialog.respondFromQnAMakerResult = function(session, qnaMakerResult){
    // Save the question
    var question = session.message.text;
    session.conversationData.userQuestion = question;

    // boolean to check if the result is formatted for a card
    var isCardFormat = qnaMakerResult.answers[0].answer.includes(';');

    if(!isCardFormat){
        // Not semi colon delimited, send a normal text response 
        session.send(qnaMakerResult.answers[0].answer);
    }else if(qnaMakerResult.answers && qnaMakerResult.score >= 0.5){
        
        var qnaAnswer = qnaMakerResult.answers[0].answer;
        
                var qnaAnswerData = qnaAnswer.split(';');
                var title = qnaAnswerData[0];
                var description = qnaAnswerData[1];
                var url = qnaAnswerData[2];
                var imageURL = qnaAnswerData[3];
        
                var msg = new builder.Message(session)
                msg.attachments([
                    new builder.HeroCard(session)
                    .title(title)
                    .subtitle(description)
                    .images([builder.CardImage.create(session, imageURL)])
                    .buttons([
                        builder.CardAction.openUrl(session, url, "Learn More")
                    ])
                ]);
        }
    session.send(msg).endDialog();
}

basicQnAMakerDialog.defaultWaitNextMessage = function(session, qnaMakerResult){
    // saves the user's question
    session.conversationData.userQuestion = session.message.text; 
    
    if(!qnaMakerResult.answers){
        let msg = new builder.Message(session)
        .addAttachment({
            contentType: "application/vnd.microsoft.card.adaptive",
            content: {
                type: "AdaptiveCard",
                body: [
                    {
                        "type": "TextBlock",
                        "text": `${session.conversationData.userQuestion}`,
                        "size": "large",
                        "weight": "bolder",
                        "color": "accent",
                        "wrap": true
                    },
                    {
                        "type": "TextBlock",
                        "text": `Sorry, no answer found in QnA service`,
                        "size": "large",
                        "weight": "regular",
                        "color": "dark",
                        "wrap": true
                    }
                ]
            }
        });
        session.send(msg);
    }
    session.endDialog();
}

bot.dialog('/', basicQnAMakerDialog);
