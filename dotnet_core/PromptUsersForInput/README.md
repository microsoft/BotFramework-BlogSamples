Sample code for **Create your own prompts to gather user input**

This sample demonstrates how to create your own prompts with ASP.Net Core 2.
The bot maintains conversation state to track and direct the conversation and ask the user questions.
The bot maintains user state to track the user's answers.

# To try this sample
- Clone the samples repository
```bash
git clone https://github.com/Microsoft/BotFramework-Samples.git
```
- [Optional] Update the `appsettings.json` file under `BotFramework-Samples\samples\dotnet_core\<sample-name>` with your botFileSecret.  For Azure Bot Service bots, you can find the botFileSecret under application settings.
# Prerequisites
## Visual Studio
- Navigate to the samples folder (`BotFramework-Samples\dotnet_core\<sample-name>`) and open \<sample-name>.csproj in Visual Studio.
- Hit F5.

## Visual Studio Code
- Open `BotFramework-Samples\dotnet_core\<sample-name>` sample folder.
- Bring up a terminal, navigate to `BotFramework-Samples\dotnet_core\<sample-name>` folder.
- Type 'dotnet run'.

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot 
developers to test and debug their bots on localhost or running remotely through a tunnel.
- Install the Bot Framework emulator from [here](https://aka.ms/botframeworkemulator).

## Connect to bot using Bot Framework Emulator **V4**
- Launch the Bot Framework Emulator.
- File -> Open bot and navigate to `BotFramework-Samples\dotnet_core\<sample-name>` folder.
- Select `BotConfiguration.bot` file.

# Bot state
A bot is inherently stateless. Once your bot is deployed, it may not run in the same process or on the same machine from one turn to the next.
However, your bot may need to track the context of a conversation, so that it can manage its behavior and remember answers to previous questions.

In this example, the bot's state is used to a track number of messages.
- We use the bot's turn handler and user and conversation state properties to manage the flow of the conversation and the collection of input.
- We ask the user a series of questions; parse, validate, and normalize their answers; and then save their input.

# Deploy this bot to Azure
You can use the [MSBot](https://github.com/microsoft/botbuilder-tools) Bot Builder CLI tool to clone and configure any services this sample depends on. 

To install all Bot Builder tools - 
```bash
npm i -g msbot chatdown ludown qnamaker luis-apis botdispatch luisgen
```
To clone this bot, run
```
msbot clone services -f deploymentScripts/msbotClone -n <BOT-NAME> -l <Azure-location> --subscriptionId <Azure-subscription-id>
```
# Further reading
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction)
- [About bot state](https://docs.microsoft.com/azure/bot-service/bot-builder-concept-state)
- [Managing conversation and user state](https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-state)
- [Prompt users for input](https://docs.microsoft.com/azure/bot-service/bot-builder-primitive-prompts)
- [Write directly to storage](https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-storage)
