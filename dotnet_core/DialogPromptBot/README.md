# Dialog Prompt Bot
 
This sample demonstrates how to use dialog prompts in your ASP.Net Core 2 bot to gather and validate user input.

# To try this sample
- Clone the samples repository
```bash
git clone https://github.com/Microsoft/botframework-samples.git
```
- [Optional] Update the `appsettings.json` file under `botframework-samples\dotnet_core\DialogPromptBot` with your botFileSecret.  For Azure Bot Service bots, you can find the botFileSecret under application settings.
# Prerequisites
## Visual Studio
- Navigate to the samples folder (`botframework-samples\dotnet_core\DialogPromptBot`) and open DialogPromptBot.csproj in Visual Studio.
- Hit F5.

## Visual Studio Code
- Open `botframework-samples\dotnet_core\DialogPromptBot` sample folder.
- Bring up a terminal, navigate to `botframework-samples\dotnet_core\DialogPromptBot` folder.
- Type 'dotnet run'.

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot 
developers to test and debug their bots on localhost or running remotely through a tunnel.
- Install the Bot Framework emulator from [here](https://aka.ms/botframeworkemulator).

## Connect to bot using Bot Framework Emulator **V4**
- Launch the Bot Framework Emulator.
- File -> Open bot and navigate to `botframework-samples\dotnet_core\DialogPromptBot` folder.
- Select `dialog-prompt.bot` file.

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
- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot State](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-storage-concept?view=azure-bot-service-4.0)
- [Managing conversation and user state](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-v4-state?view=azure-bot-service-4.0&tabs=js)
