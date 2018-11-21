# Dialog Prompt Bot
 
This sample demonstrates how to use dialog prompts in your ASP.Net Core 2 bot to gather and validate user input.

# To try this sample
- Clone the samples repository
```bash
git clone https://github.com/Microsoft/BotFramework-Samples.git
```
- [Optional] Update the `appsettings.json` file under `BotFramework-Samples\dotnet_core\DialogPromptBot` with your botFileSecret.  For Azure Bot Service bots, you can find the botFileSecret under application settings.
# Prerequisites
## Visual Studio
- Navigate to the samples folder (`BotFramework-Samples\SDKV4-Samples\dotnet_core\DialogPromptBot`) and open DialogPromptBot.csproj in Visual Studio.
- Hit F5.

## Visual Studio Code
- Open `BotFramework-Samples\SDKV4-Samples\dotnet_core\DialogPromptBot` sample folder.
- Bring up a terminal, navigate to `BotFramework-Samples\SDKV4-Samples\dotnet_core\DialogPromptBot` folder.
- Type 'dotnet run'.

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot 
developers to test and debug their bots on localhost or running remotely through a tunnel.
- Install the Bot Framework emulator from [here](https://aka.ms/botframeworkemulator).

## Connect to bot using Bot Framework Emulator **V4**
- Launch the Bot Framework Emulator.
- File -> Open bot and navigate to `BotFramework-Samples\SDKV4-Samples\dotnet_core\DialogPromptBot` folder.
- Select `dialog-prompt.bot` file.

# Further reading
- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot State](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-storage-concept?view=azure-bot-service-4.0)
- [Managing conversation and user state](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-v4-state?view=azure-bot-service-4.0&tabs=js)
