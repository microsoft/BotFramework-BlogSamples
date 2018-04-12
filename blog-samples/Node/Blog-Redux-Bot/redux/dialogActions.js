const PROMPT_TEXT = 'DIALOG/PROMPT_TEXT';
const RECEIVE_MESSAGE = 'DIALOG/RECEIVE_MESSAGE';
const SEND_EVENT = 'DIALOG/SEND_EVENT';
const SEND_MESSAGE = 'DIALOG/SEND_MESSAGE';
const END_CONVERSATION = 'DIALOG/END_CONVERSATION';

function promptText(text) {
  return { type: PROMPT_TEXT, payload: { text } };
}

function receiveMessage(text, attachments, result) {
  return {
    type: RECEIVE_MESSAGE,
    payload: { attachments, result, text }
  };
}

function sendEvent(name, value) {
  return {
    type: SEND_EVENT,
    payload: { name, value }
  };
}

function sendMessage(text, attachments) {
  return {
    type: SEND_MESSAGE,
    payload: { attachments, text }
  };
}

function endConversation(text, attachments, result){
  return {
    type: END_CONVERSATION,
    payload: { attachments, result, text }
  };
}

module.exports = {
  PROMPT_TEXT,
  RECEIVE_MESSAGE,
  SEND_EVENT,
  SEND_MESSAGE,
  END_CONVERSATION,

  promptText,
  receiveMessage,
  sendEvent,
  sendMessage,
  endConversation
};
