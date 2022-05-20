import { connectRouter } from 'connected-react-router';
import { combineReducers } from 'redux';
import { enableBatching } from 'redux-batched-actions';
import actions from 'Store/Actions';

const defaultState = {};
const reducers = {};

actions.forEach((action) => {
  const section = action.section;

  defaultState[section] = action.defaultState;
  reducers[section] = action.reducers;
});

export { defaultState };

export default function(history) {
  return enableBatching(combineReducers({
    ...reducers,
    router: connectRouter(history)
  }));
}
