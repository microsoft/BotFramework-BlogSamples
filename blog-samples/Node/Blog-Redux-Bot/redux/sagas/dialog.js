const builder = require('botbuilder');
const { takeEvery } = require('redux-saga/effects');
const DialogActions = require('../dialogActions');

module.exports = function* (session) {
  yield takeEvery(DialogActions.PROMPT_TEXT, function* (action) {
    builder.Prompts.text(session, action.payload.text);
  });

  yield takeEvery(DialogActions.END_CONVERSATION, function* (action){
    
    session.endConversation('Bye!');
  });

  yield takeEvery(DialogActions.SEND_EVENT, function* (action) {
    const { name, value } = action.payload;

    session.send({ type: 'event', name, value });
  });

  yield takeEvery(DialogActions.SEND_MESSAGE, function* (action) {
    const { attachments, text } = action.payload;

    message = new builder.Message(session);

    text && message.text(text);
    attachments && message.attachments(attachments);

    session.send(message);
  });
};
