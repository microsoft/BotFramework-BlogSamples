const RESET = 'RESET';
const SET_CITY = 'SET_CITY';
const SET_USERNAME = 'SET_USERNAME';

function reset() {
  return { type: RESET };
}

function setCity(city) {
  return { type: SET_CITY, payload: { city } };
}

function setUsername(username) {
  return { type: SET_USERNAME, payload: { username } };
}

module.exports = {
  RESET,
  SET_CITY,
  SET_USERNAME,

  reset,
  setCity,
  setUsername
};
