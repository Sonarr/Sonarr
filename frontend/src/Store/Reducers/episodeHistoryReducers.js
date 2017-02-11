import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import createSetReducer from './Creators/createSetReducer';
import createUpdateReducer from './Creators/createUpdateReducer';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: []
};

const reducerSection = 'episodeHistory';

const episodeHistoryReducers = handleActions({

  [types.SET]: createSetReducer(reducerSection),
  [types.UPDATE]: createUpdateReducer(reducerSection),

  [types.CLEAR_EPISODE_HISTORY]: (state) => {
    return Object.assign({}, state, defaultState);
  }

}, defaultState);

export default episodeHistoryReducers;
