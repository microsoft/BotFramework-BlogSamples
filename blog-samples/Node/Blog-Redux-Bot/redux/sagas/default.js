const { put, select, takeEvery } = require('redux-saga/effects');
const { reset, setCity, setUsername } = require('../conversationActions');
const { promptText, RECEIVE_MESSAGE, sendMessage, endConversation } = require('../dialogActions');

module.exports = function* (session) {
  yield takeEvery(RECEIVE_MESSAGE, function* (action) {
    const { text } = action.payload;
    const changeCityMatch = /^change city to (.*)/i.exec(text);
    const currentCityMatch = /^current city/i.exec(text);
    const resetMatch = /^reset/i.exec(text);
    const endConversationMatch = /^end conversation/i.exec(text);
    let { city, username } = yield select();

    if (!city) {
      city = 'Seattle';

      yield put(setCity(city));
      yield put(sendMessage(`Welcome to the Search City bot. I\'m currently configured to search for things in ${ city }`));
      yield put(promptText('Before get started, please tell me your name?'));
    } else if (!username) {
      yield put(setUsername(text));
      yield put(sendMessage(`Welcome ${ text }!\n * If you want to know which city I'm using for my searches type 'current city'. \n * Want to change the current city? Type 'change city to cityName'. \n * Want to change it just for your searches? Type 'change my city to cityName'`));
    } else if (changeCityMatch) {
      const newCity = changeCityMatch[1];

      yield put(setCity(newCity));
      yield put(sendMessage(`All set ${ username }. From now on, all my searches will be for things in ${ newCity }.`));
    } else if (currentCityMatch) {
      yield put(sendMessage(`Hey ${ username }, I\'m currently configured to search for things in ${ city }.`));
    } else if (resetMatch) {
      yield put(reset());
      yield put(sendMessage('Oops... I\'m suffering from a memory loss...'));
    } else if (endConversationMatch){
      yield put(endConversation());
      yield put(sendMessage('Ending Conversation...'));
    } else {
      const { city, username } = yield select();
      const messageText = action.payload.text.trim();

      yield put(sendMessage(`${ username }, wait a few seconds. Searching for \'${ messageText }\' in \'${ city }\'...`));
      yield put(sendMessage(`https://www.bing.com/search?q=${ encodeURIComponent(`${ messageText } in ${ city }`) }`));
    }
  });
};
