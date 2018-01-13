import { combineReducers } from 'redux';
import { enableBatching } from 'redux-batched-actions';
import { routerReducer } from 'react-router-redux';
import actions from 'Store/Actions';

const defaultState = {};

const reducers = {
  routing: routerReducer
};

actions.forEach((action) => {
  const section = action.section;

  defaultState[section] = action.defaultState;
  reducers[section] = action.reducers;
});

export { defaultState };
export default enableBatching(combineReducers(reducers));
