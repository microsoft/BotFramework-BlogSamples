# Redux-Bot
![redux-abs-logo][redux-abs-logo]

BotBuilder v3 Node.js bot with Redux state management

# Overview

This sample is to showcase the flexibility of the Azure Bot Service, by demonstrating that you can author rich conversational experiences using whatever technologies/libraries you'd like. This sample bot uses [Redux](https://redux.js.org/), a popular JavaScript framework for application state management, and [Redux-Saga](https://redux-saga.js.org/) to recreate an existing [city search bot sample](https://github.com/Microsoft/BotBuilder-Samples/tree/master/Node/core-CustomState). 

![city-search-bot][city-search-bot]

# To run this sample

Clone BotFramework-Samples repo, and CD into this folder (Blog-Redux-Bot). From your CLI, install the node_module dependencies:

```
npm install
```

## Emulator

This bot runs by default on **localhost:3978**, which you can run directly using the [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator).

```
node app.js 
```

## UI Redux Store Render

![ui-state][ui-state]

Included in **public/index.html** is a simple web app using a custom web chat instance which communicates with the Azure Bot Service over the DirectLine channel.  

[Official ngrok page](https://ngrok.com/)

![ngrok][ngrok]

On Azure, in bot channels registration, copy and paste the ngrok port forwarding address to the messaging endpoint in settings--> configuration as shown. **Ensure that this address ends in /api/messages.**

![channel-reg][channel-registration]

In your CLI, navigate to the redux bot root folder and run:

```
npm start
```

This will run the custom web application on **localhost:3000**.
Lastly, provision the [DirectLine Secret](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-channel-connect-directline) in the URL header of your browser. This will allow the Azure Bot Service to communicate with your local hosted custom web chat instance.  

![dl-secret][dl-secret]

Now, when you interact with the bot, any state changes to the Redux Store will be rendered on the web page. 


# To-do

* [ ] More tests for Redux
* [X] Cleaner code inside `app.js`
* [ ] Investigate possibility to use ChatConnector directly

# Hiccups

## Turning `conversationUpdate` event into an action

It would be great if a new member joined, we sent a welcome message right away.

* `bot.on('conversationUpdate')` event handler does not associate with `session` object
* `session` object is required to create a Redux store

Looking at other sample code on the same scenario, instead of sending the greeting thru `session.send`, it must be sent thru `bot.send` with an addressed message. Thus, it further proves that `conversationUpdate` is not associated with any `session` object.

Because our Redux store design requires `session` object, thus, `conversationUpdate` cannot be turn into an action.

## Multi-turn dialog

It is intuitive to write code in `redux-saga` like this:

```js
takeEvery(RECEIVE_MESSAGE, function* (action) {
  yield put(promptText('What is your name?'));

  action = yield take(RECEIVE_RESULT);

  yield put(sendMessage(`Hello, ${ action.payload.response }`);
});
```

But this would require a dialog to resume in the middle of a saga (resume at the `yield take` line). Due to the nature of serverless functions, it is difficult to implement a saga that works this way.

# References
* [Redux](https://github.com/reactjs/redux)
* [Redux-Saga](https://github.com/redux-saga/redux-saga)
* [BotBuilder SDK](https://github.com/Microsoft/BotBuilder)
* [ngrok](https://ngrok.com/)

[redux-abs-logo]: ../../images/redux-abs-logo.png
[ngrok]: ../../images/ngrok-forward.png
[city-search-bot]: ../../images/redux-bot-02.png
[channel-registration]: ../../images/bot-channels-ngrok.png
[bot-webchat]: ../../images/redux-bot-01.png
[ui-state]: ../../images/redux-store-02.png
[dl-secret]: ../../images/direct-line-secret-url.png