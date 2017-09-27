# TriviaBotSpeechSample
A sample trivia bot, leveraging the Microsoft Bot Framework and https://opentdb.com/ that showcases the use of the new speech-enabled Microsoft.Bot.Client NuGet package and Microsoft Bot Framework C# Builder SDK features.

This sample contains two projects, a trivia bot built on top of the Microsoft Bot Framework C# Builder SDK, and a UWP app that talks to the bot using the Microsoft.Bot.Client NuGet package.

As this is a combined client/sample demo, there is a bit of setup required.

Setup:
1) Register a bot with the Bot Framework at http://dev.botframework.com/ and add the AppId and AppPassword to TriviaBot\Web.config.
2) Enable the Direct Line channel in the bot settings page, Add a new site and paste a Direct Line secret in TriviaApp\BotConnection.cs.
3) Create a LUIS app on http://luis.ai/ and import TriviaBotLU.json as a new app. Train and publish the model, and add the app id and subscription key in TriviaBot\TriviaDialog.cs (there is an error pointing to the location).
    The LUIS app id and subscription key can be extracted from the Endpoint Url provided on the "Publish App" page at http://luis.ai/
    The link format is: https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/yourappid?subscription-key=yoursubscriptionkey&verbose=true&q=.
4) Publish the Bot as an Azure App Service, and add the public endpoint (yourhosturl/api/messages) to the Bot Framework portal settings page. Make sure to use https instead of http in the url.
5) [Optional] To improve speech recognition for your bot: On  http://dev.botframework.com/ go to the bot's settings. In the "Speech recognition priming with LUIS" section you should see a list of LUIS apps associated with the account you are logged in with. Check the new LUIS app you created for this bot and hit save. This information is used to improve speech recognition when you speak to this bot and uses the Cognitive Speech apis for speech recognition. Speech recognition priming improves the recognition accuracy for the utterances and entities defined in your LUIS app for this bot.

To start a conversation with this bot, you can say something like "let's play trivia" or "let's play geography trivia"

You can talk to your new bot in multiple ways. Here are some options to try, all of which support speech input and output:
1) Using the TriviaApp included in this sample. Simply hit F5 in Visual Studio 
2) Using the bot framework emulator https://docs.microsoft.com/en-us/bot-framework/debug-bots-emulator.
2) Host your own instance of the Bot Framework WebChat client:  https://aka.ms/BfWebChat    
3) Enabled your bot as a Cortana Skill. Simply enable the Cortana channel and provide an invocation phrase. Then make sure you are logged in to cortana using the same microsoft account, and say "Ask <invocation name> to start a game of trivia". Cortana should trigger your bot!
