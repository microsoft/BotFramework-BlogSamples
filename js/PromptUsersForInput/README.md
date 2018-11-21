# simplePrompts
Sample code for **Create your own prompts to gather user input**

This bot has been created using [Microsoft Bot Framework][bot-service-docs].
It demonstrates how to create your own prompts.
The bot maintains conversation state to track and direct the conversation and ask the user questions.
The bot maintains user state to track the user's answers.
This bot example uses [`restify`][restify].

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
[Microsoft Bot Framework Emulator][emulator] is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework emulator from [here][3]

## Connect to bot using Bot Framework Emulator **V4**
- Launch Bot Framework Emulator
- File -> Open Bot Configuration
- Select `<bot-config>.bot` file

# Bot state
A bot is inherently stateless. Once your bot is deployed, it may not run in the same process or on the same machine from one turn to the next.
However, your bot may need to track the context of a conversation, so that it can manage its behavior and remember answers to previous questions.

In this example, the bot's state is used to a track number of messages.
- We use the bot's turn handler and user and conversation state properties to manage the flow of the conversation and the collection of input.
- We ask the user a series of questions; parse, validate, and normalize their answers; and then save their input.

# Deploy this bot to Azure
You can use the [MSBot][cli-tools] Bot Builder CLI tool to clone and configure the services this sample depends on.

To install all Bot Builder tools -

Ensure you have [Node.js](https://nodejs.org/) version 8.5 or higher

```bash
npm i -g msbot chatdown ludown qnamaker luis-apis botdispatch luisgen
```

To clone this bot, run
```
msbot clone services -f deploymentScripts/msbotClone -n myChatBot -l <Azure-location> --subscriptionId <Azure-subscription-id>
```

# Further reading
- [Azure Bot Service introduction][bot-service-overview]
- [About bot state][state-concept]
- [Prompt users for input][primitive-prompts]
- [Managing conversation and user state][state-how-to]
- [Write directly to storage][storage-how-to]


[restify]: https://www.npmjs.com/package/restify
[emulator]: https://aka.ms/botframework-emulator
[cli-tools]: https://github.com/microsoft/botbuilder-tools

[bot-service-docs]: https://docs.microsoft.com/azure/bot-service/
[bot-service-overview]: https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction
[state-concept]: https://docs.microsoft.com/azure/bot-service/bot-builder-concept-state
[primitive-prompts]: https://docs.microsoft.com/azure/bot-service/bot-builder-primitive-prompts
[state-how-to]: https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-state
[storage-how-to]: https://docs.microsoft.com/azure/bot-service/bot-builder-howto-v4-storage