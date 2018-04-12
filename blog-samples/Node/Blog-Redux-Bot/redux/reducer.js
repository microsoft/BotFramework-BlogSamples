const { RESET, SET_CITY, SET_USERNAME } = require('./conversationActions');

const DEFAULT_STATE = {
  city: null,
  username: null
};

function conversationReducer(state = DEFAULT_STATE, action) {
  switch (action.type) {
  case RESET:
    state = DEFAULT_STATE;
    break;

  case SET_CITY:
    state = { ...state, city: action.payload.city };
    break;

  case SET_USERNAME:
    state = { ...state, username: action.payload.username };
    break;
  }

  return state;
}

module.exports = conversationReducer;
