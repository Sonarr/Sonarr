import { createStore } from 'redux';
import reducers, { defaultState } from 'Store/Reducers';
import middlewares from 'Store/Middleware/middlewares';

function createAppStore(history) {
  const appStore = createStore(
    reducers,
    defaultState,
    middlewares(history)
  );

  return appStore;
}

export default createAppStore;
