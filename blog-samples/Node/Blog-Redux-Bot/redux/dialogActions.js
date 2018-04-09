const PROMPT_TEXT = 'DIALOG/PROMPT_TEXT';
const RECEIVE_MESSAGE = 'DIALOG/RECEIVE_MESSAGE';
const SEND_EVENT = 'DIALOG/SEND_EVENT';
const SEND_MESSAGE = 'DIALOG/SEND_MESSAGE';

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

module.exports = {
  PROMPT_TEXT,
  RECEIVE_MESSAGE,
  SEND_EVENT,
  SEND_MESSAGE,

  promptText,
  receiveMessage,
  sendEvent,
  sendMessage
};
