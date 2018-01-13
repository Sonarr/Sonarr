import { createAction } from 'redux-actions';
import requestAction from 'Utilities/requestAction';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import { createThunk, handleThunks } from 'Store/thunks';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'captcha';

//
// State

export const defaultState = {
  refreshing: false,
  token: null,
  siteKey: null,
  secretToken: null,
  ray: null,
  stoken: null,
  responseUrl: null
};

//
// Actions Types

export const REFRESH_CAPTCHA = 'captcha/refreshCaptcha';
export const GET_CAPTCHA_COOKIE = 'captcha/getCaptchaCookie';
export const SET_CAPTCHA_VALUE = 'captcha/setCaptchaValue';
export const RESET_CAPTCHA = 'captcha/resetCaptcha';

//
// Action Creators

export const refreshCaptcha = createThunk(REFRESH_CAPTCHA);
export const getCaptchaCookie = createThunk(GET_CAPTCHA_COOKIE);
export const setCaptchaValue = createAction(SET_CAPTCHA_VALUE);
export const resetCaptcha = createAction(RESET_CAPTCHA);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [REFRESH_CAPTCHA]: function(getState, payload, dispatch) {
    const actionPayload = {
      action: 'checkCaptcha',
      ...payload
    };

    dispatch(setCaptchaValue({
      refreshing: true
    }));

    const promise = requestAction(actionPayload);

    promise.done((data) => {
      if (!data.captchaRequest) {
        dispatch(setCaptchaValue({
          refreshing: false
        }));
      }

      dispatch(setCaptchaValue({
        refreshing: false,
        ...data.captchaRequest
      }));
    });

    promise.fail(() => {
      dispatch(setCaptchaValue({
        refreshing: false
      }));
    });
  },

  [GET_CAPTCHA_COOKIE]: function(getState, payload, dispatch) {
    const state = getState().captcha;

    const queryParams = {
      responseUrl: state.responseUrl,
      ray: state.ray,
      captchaResponse: payload.captchaResponse
    };

    const actionPayload = {
      action: 'getCaptchaCookie',
      queryParams,
      ...payload
    };

    const promise = requestAction(actionPayload);

    promise.done((data) => {
      dispatch(setCaptchaValue({
        token: data.captchaToken
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_CAPTCHA_VALUE]: function(state, { payload }) {
    const newState = Object.assign(getSectionState(state, section), payload);

    return updateSectionState(state, section, newState);
  },

  [RESET_CAPTCHA]: function(state) {
    return updateSectionState(state, section, defaultState);
  }

}, defaultState);
