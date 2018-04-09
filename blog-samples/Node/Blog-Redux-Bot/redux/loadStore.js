const { applyMiddleware, createStore } = require('redux');
const { default: createSagaMiddleware } = require('redux-saga');

const createDefaultSaga = require('./sagas/default');
const createDialogSagas = require('./sagas/dialog');
const reducer = require('./reducer');

module.exports = function loadStore(session) {
  const saga = createSagaMiddleware();
  const store = createStore(
    reducer,

    // Restore the store from conversationData
    session.conversationData,

    applyMiddleware(
      saga,
      store => next => action => {
        // Send action to web page for debugging
        session.send({
          type: 'event',
          name: 'action',
          value: action
        });

        return next(action);
      }
    )
  );

  store.subscribe(() => {
    // Save the store to conversationData
    session.conversationData = store.getState();
    session.save();

    // Send store state to web page for debugging
    session.send({
      type: 'event',
      name: 'store',
      value: store.getState()
    });
  });

  saga.run(function* () {
    yield* createDialogSagas(session);
    yield* createDefaultSaga(session);
  });

  return store;
};
