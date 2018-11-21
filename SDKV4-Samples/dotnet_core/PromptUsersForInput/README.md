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
- Open `BotFramework-Samples\SDKV4-Samples\dotnet_core\<sample-name>` sample folder.
- Bring up a terminal, navigate to `BotFramework-Samples\dotnet_core\<sample-name>` folder.
- Type 'dotnet run'.

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot 
developers to test and debug their bots on localhost or running remotely through a tunnel.
- Install the Bot Framework emulator from [here](https://aka.ms/botframeworkemulator).

## Connect to bot using Bot Framework Emulator **V4**
- Launch the Bot Framework Emulator.
- File -> Open bot and navigate to `BotFramework-Samples\SDKV4-Samples\dotnet_core\<sample-name>` folder.
- Select `custom-prompt.bot` file.

# Further reading
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction)
