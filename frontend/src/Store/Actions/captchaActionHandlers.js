import requestAction from 'Utilities/requestAction';
import * as types from './actionTypes';
import { setCaptchaValue } from './captchaActions';

const captchaActionHandlers = {
  [types.REFRESH_CAPTCHA]: function(payload) {
    return (dispatch, getState) => {
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
    };
  },

  [types.GET_CAPTCHA_COOKIE]: function(payload) {
    return (dispatch, getState) => {
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
    };
  }
};

export default captchaActionHandlers;
