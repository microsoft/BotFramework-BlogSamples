const builder = require('botbuilder');
const cognitiveServices = require('./cognitiveservices');

const buildResultsMessageWithAttachments = (session, resultsArray) => {
    const attachments = [];

    const message = new builder.Message(session);
    message.attachmentLayout(builder.AttachmentLayout.carousel);

    //Just to be safe, skype and teams have a card limit of 6/10
    let limit = (resultsArray.length > 6) ? 6 : resultsArray.length;

    for (let i = 0; i < limit; i++) {
        const result = resultsArray[i];

        const attachment = {
            contentType: "application/vnd.microsoft.card.adaptive",
            content: {
                type: "AdaptiveCard",
                body: [
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "size": 2,
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "text": `${result.title}`,
                                        "weight": "bolder",
                                        "size": "large",
                                        "wrap": true,
                                    },
                                    {
                                        "type": "TextBlock",
                                        "text": `${result.body_markdown}`,
                                        "size": "normal",
                                        "horizontalAlignment": "left",
                                        "wrap": true,
                                        "maxLines": 5,
                                    }
                                ]
                            }
                        ]
                    }
                ],
                actions: [
                    {
                        "type": "Action.OpenUrl",
                        "title": "Find out more",
                        "url": `${result.link}`
                    }
                ]
            }
        }

        attachments.push(attachment);
    }

    message.attachments(attachments);
    return message;
};

const buildMenuMessageWithAttachments = (session) => {
    const attachments = [];
    const message = new builder.Message(session);
    message.attachmentLayout(builder.AttachmentLayout.carousel);

    const mainMenu = [
        {
            title: "Ask a Question",
            text: "Ask any programming question and I will find it for you on Stack Overflow!",
            buttonText: "I want to ask a question… 🤔",
        },
        {
            title: "Help with a Screenshot",
            text: "Send me a screenshot of your exception dialog and I will find a solution for you on Stack Overflow!",
            buttonText: "I need help with a screenshot… 🤔",
        },
        {
            title: "Tell a Joke",
            text: "Over the years, I have collected a few developer jokes; want to hear one?",
            buttonText: "Brighten my day 😀",
        }
    ];

    for (let i = 0; i < mainMenu.length; i++) {
        const result = mainMenu[i];
        const attachment = {
            contentType: "application/vnd.microsoft.card.adaptive",
            content: {
                type: "AdaptiveCard",
                body: [
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "size": 2,
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "text": `${result.title}`,
                                        "weight": "bolder",
                                        "size": "large",
                                        "wrap": true,
                                    },
                                    {
                                        "type": "TextBlock",
                                        "text": `${result.text}`,
                                        "size": "normal",
                                        "horizontalAlignment": "left",
                                        "wrap": true,
                                        "maxLines": 5,
                                    }
                                ]
                            }
                        ]
                    }
                ],
                actions: [
                    {
                        "type": "Action.Submit",
                        "title": `${result.buttonText}`,
                        "data":  `${result.buttonText}`
                    }
                ]
            }
        }

        attachments.push(attachment);
    }

    message.attachments(attachments);
    return message;
};

const buildCongitiveServicesMessageWithAttachments = (session) => {
    const attachments = [];

    const message = new builder.Message(session);
    message.attachmentLayout(builder.AttachmentLayout.carousel);

    for (let i = 0; i < cognitiveServices.length; i++) {
        const result = cognitiveServices[i];

        const attachment = {
            contentType: "application/vnd.microsoft.card.adaptive",
            content: {
                type: "AdaptiveCard",
                body: [
                    {
                        "type": "ColumnSet",
                        "columns": [
                            {
                                "type": "Column",
                                "size": 2,
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "text": `${result.title}`,
                                        "weight": "bolder",
                                        "size": "large",
                                        "wrap": true,
                                    },
                                    {
                                        "type": "TextBlock",
                                        "text": `${result.description}`,
                                        "size": "normal",
                                        "horizontalAlignment": "left",
                                        "wrap": true,
                                        "maxLines": 5,
                                    }
                                ]
                            },
                            {
                                "type": "Column",
                                "size": 1,
                                "items": [
                                    {
                                        "type": "Image",
                                        "url": "https://docs.microsoft.com/en-us/azure/cognitive-services/media/index/cognitive-services.svg",
                                        "size": "small",
                                        "horizontalAlignment": "right",
                                    }
                                ]
                            }
                        ]
                    }
                ],
                actions: [
                    {
                        "type": "Action.OpenUrl",
                        "title": "Learn More",
                        "url": `${result.url}`
                    }
                ]
            }
        }

        attachments.push(attachment);
    }

    message.attachments(attachments);
    return message;
};

module.exports = {
    buildMenuMessageWithAttachments,
    buildResultsMessageWithAttachments,
    buildCongitiveServicesMessageWithAttachments
}