# Save user and conversation data

This sample demonstrates how to save user and conversation data in a Node.js bot.
The bot maintains conversation state to track and direct the conversation and ask the user questions.
The bot maintains user state to track the user's answers.

# To run the bot

- Install modules and start the bot
    ```bash
    npm i & npm start
    ```
    Alternatively you can also use nodemon via
    ```bash
    npm i & npm run watch
    ```

# Testing the bot using Bot Framework Emulator

[Microsoft Bot Framework Emulator][2] is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework emulator from [here][3]

## Connect to bot using Bot Framework Emulator **V4**

- Launch Bot Framework Emulator
- File -> Open Bot Configuration
- Select `stateBot.bot` file

# Further reading

- [Azure Bot Service Introduction][6]
- [Bot State][7]
- [Write directly to storage][8]
- [Managing conversation and user state][9]

[1]: https://www.npmjs.com/package/restify
[2]: https://github.com/microsoft/botframework-emulator
[3]: https://aka.ms/botframework-emulator
[4]: https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-state?tabs=js
[5]: https://github.com/microsoft/botbuilder-tools
[6]: https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction
[7]: https://docs.microsoft.com/azure/bot-service/bot-builder-storage-concept
[8]: https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-storage?tabs=js
[9]: https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-state?tabs=js
[10] https://dev.botframework.com