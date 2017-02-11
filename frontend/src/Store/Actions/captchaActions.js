import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import captchaActionHandlers from './captchaActionHandlers';

export const refreshCaptcha = captchaActionHandlers[types.REFRESH_CAPTCHA];
export const getCaptchaCookie = captchaActionHandlers[types.GET_CAPTCHA_COOKIE];
export const setCaptchaValue = createAction(types.SET_CAPTCHA_VALUE);
export const resetCaptcha = createAction(types.RESET_CAPTCHA);
