import { createStore } from 'redux';
import createReducers, { defaultState } from 'Store/Actions/createReducers';
import middlewares from 'Store/Middleware/middlewares';

function createAppStore() {
  const appStore = createStore(
    createReducers(),
    defaultState,
    middlewares()
  );

  return appStore;
}

export default createAppStore;
