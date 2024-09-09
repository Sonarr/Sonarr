import { createStore } from 'redux';
import createReducers, { defaultState } from 'Store/Actions/createReducers';
import middlewares from 'Store/Middleware/middlewares';

function createAppStore(history) {
  const appStore = createStore(
    createReducers(history),
    defaultState,
    middlewares(history)
  );

  return appStore;
}

export default createAppStore;
