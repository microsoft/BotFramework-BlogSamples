/*
 * Botbuilder v4 SDK - Rich Media Attachments
 * 
 * This bot demonstrates multiple types of rich media attachments.
 * 
 * To run this bot:
 * 1) install these npm packages:
 * npm install --save restify
 * npm install --save botbuilder@preview
 * 
 * 2) From VSCode, open the package.json file and make sure that "main" is not set to any path (or is undefined) 
 * 3) Navigate to your bot app.js file and run the bot in debug mode (eg: click Debug/Start debuging)
 * 4) Load the emulator and point it to: http://localhost:3978/api/messages
 * 5) Send the message "hi" to engage with the bot.
 *
 */ 

// Packages are installed for you
const { BotFrameworkAdapter, MessageFactory, CardFactory, ActionTypes } = require('botbuilder');
const restify = require('restify');

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

// Listen for incoming requests 
server.post('/api/messages', (req, res) => {
    // Route received request to adapter for processing
    adapter.processActivity(req, res, async (context) => {
        // This bot is only handling Messages
        if (context.activity.type === 'message') {

            // To send the user a single piece of content like an image or a video, you can send media contained in a URL:
            let imageOrVideoMessage = MessageFactory.contentUrl('https://images.pexels.com/photos/248797/pexels-photo-248797.jpeg?auto=compress&cs=tinysrgb&h=350', 'image/jpeg')

            // Send the activity to the user.
            await context.sendActivity(imageOrVideoMessage);

            // To send a list of attachments, stacked one on top of another:
            let messageWithCarouselOfCardsList = MessageFactory.list([
                CardFactory.heroCard('title1', ['imageUrl1'], ['button1']),
                CardFactory.heroCard('title2', ['imageUrl2'], ['button2']),
                CardFactory.heroCard('title3', ['imageUrl3'], ['button3'])
            ]);

            await context.sendActivity(messageWithCarouselOfCardsList);

            // To send the user a card and button, you can attach a heroCard to the message:
            const message = MessageFactory.attachment(
                CardFactory.heroCard(
                    'White T-Shirt',
                    ['https://example.com/whiteShirt.jpg'],
                    ['buy']
                )
            );

            await context.sendActivity(message);

            // The following code shows examples using various rich card events.

            const hero = MessageFactory.attachment(
                CardFactory.heroCard(
                    'Holler Back Buttons',
                    ['https://example.com/whiteShirt.jpg'],
                    [{
                        type: ActionTypes.ImBack,
                        title: 'ImBack',
                        value: 'You can ALL hear me! Shout Out Loud'
                    },
                    {
                        type: ActionTypes.PostBack,
                        title: 'PostBack',
                        value: 'Shh! My Bot friend hears me. Much Quieter'
                    },
                    {
                        type: ActionTypes.OpenUrl,
                        title: 'OpenUrl',
                        value: 'https://en.wikipedia.org/wiki/{cardContent.Key}'
                    }]
                )
            );

            await context.sendActivity(hero);

            // Adaptive Card

            const adaptive_message = CardFactory.adaptiveCard({
                "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                "version": "1.0",
                "type": "AdaptiveCard",
                "speak": "Your flight is confirmed for you from San Francisco to Amsterdam on Friday, October 10 8:30 AM",
                "body": [
                    {
                        "type": "TextBlock",
                        "text": "Passenger",
                        "weight": "bolder",
                        "isSubtle": false
                    },
                    {
                        "type": "TextBlock",
                        "text": "Sarah Hum",
                        "separator": true
                    },
                    {
                        "type": "TextBlock",
                        "text": "1 Stop",
                        "weight": "bolder",
                        "spacing": "medium"
                    },
                    {
                        "type": "TextBlock",
                        "text": "Fri, October 10 8:30 AM",
                        "weight": "bolder",
                        "spacing": "none"
                    },
                    {
                        "type": "ColumnSet",
                        "separator": true,
                        "columns": [
                            {
                                "type": "Column",
                                "width": 1,
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "text": "San Francisco",
                                        "isSubtle": true
                                    },
                                    {
                                        "type": "TextBlock",
                                        "size": "extraLarge",
                                        "color": "accent",
                                        "text": "SFO",
                                        "spacing": "none"
                                    }
                                ]
                            },
                            {
                                "type": "Column",
                                "width": "auto",
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "text": " "
                                    },
                                    {
                                        "type": "Image",
                                        "url": "http://messagecardplayground.azurewebsites.net/assets/airplane.png",
                                        "size": "small",
                                        "spacing": "none"
                                    }
                                ]
                            },
                            {
                                "type": "Column",
                                "width": 1,
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "horizontalAlignment": "right",
                                        "text": "Amsterdam",
                                        "isSubtle": true
                                    },
                                    {
                                        "type": "TextBlock",
                                        "horizontalAlignment": "right",
                                        "size": "extraLarge",
                                        "color": "accent",
                                        "text": "AMS",
                                        "spacing": "none"
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        "type": "ColumnSet",
                        "spacing": "medium",
                        "columns": [
                            {
                                "type": "Column",
                                "width": "1",
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "text": "Total",
                                        "size": "medium",
                                        "isSubtle": true
                                    }
                                ]
                            },
                            {
                                "type": "Column",
                                "width": 1,
                                "items": [
                                    {
                                        "type": "TextBlock",
                                        "horizontalAlignment": "right",
                                        "text": "$1,032.54",
                                        "size": "medium",
                                        "weight": "bolder"
                                    }
                                ]
                            }
                        ]
                    }
                ]
            });

            // send adaptive card as attachment 
            await context.sendActivity({ attachments: [adaptive_message] })


            // Send a carousel of cards
            //  init message object
            let messageWithCarouselOfCards = MessageFactory.carousel([
                CardFactory.heroCard('title1', ['imageUrl1'], ['button1']),
                CardFactory.heroCard('title2', ['imageUrl2'], ['button2']),
                CardFactory.heroCard('title3', ['imageUrl3'], ['button3'])
            ]);

            await context.sendActivity(messageWithCarouselOfCards);
        }
    });
});