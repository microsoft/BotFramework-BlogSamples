# ComplexDialogBot

This sample creates a complex conversation with dialogs and Node.js.

This bot has been created using [Microsoft Bot Framework][10], it shows how to create a simple echo bot with state. The bot maintains a simple counter that increases with each message from the user. This bot example uses [`restify`][1].

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
- Select `ComplexDialogBot.bot` file

# Further reading

- [Azure Bot Service Introduction][6]
- [Bot State][7]
- [Write directly to storage][8]
- [Managing conversation and user state][9]

[1]: https://www.npmjs.com/package/restify
[2]: https://github.com/microsoft/botframework-emulator
[3]: https://aka.ms/botframework-emulator
[6]: https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction
[7]: https://docs.microsoft.com/azure/bot-service/bot-builder-storage-concept
[8]: https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-storage?tabs=javascript
[9]: https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-state?tabs=javascript
[10] https://dev.botframework.com