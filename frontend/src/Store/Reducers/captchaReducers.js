import { handleActions } from 'redux-actions';
import * as types from 'Store/Actions/actionTypes';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';

export const defaultState = {
  refreshing: false,
  token: null,
  siteKey: null,
  secretToken: null,
  ray: null,
  stoken: null,
  responseUrl: null
};

const section = 'captcha';

const captchaReducers = handleActions({

  [types.SET_CAPTCHA_VALUE]: function(state, { payload }) {
    const newState = Object.assign(getSectionState(state, section), payload);

    return updateSectionState(state, section, newState);
  },

  [types.RESET_CAPTCHA]: function(state) {
    return updateSectionState(state, section, defaultState);
  }

}, defaultState);

export default captchaReducers;
