import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

export const defaultState = {
  authorizing: false,
  accessToken: null,
  accessTokenSecret: null
};

const section = 'oAuth';

const oAuthReducers = handleActions({

  [types.SET_OAUTH_VALUE]: function(state, { payload }) {
    const newState = Object.assign(getSectionState(state, section), payload);

    return updateSectionState(state, section, newState);
  },

  [types.RESET_OAUTH]: function(state) {
    return updateSectionState(state, section, defaultState);
  }

}, defaultState);

export default oAuthReducers;
