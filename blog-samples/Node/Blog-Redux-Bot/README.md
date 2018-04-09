# Redux-Bot
![redux-abs-logo][redux-abs-logo]

BotBuilder v3 Node.js bot with Redux state management

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

[redux-abs-logo]: ../../images/redux-abs-logo.png