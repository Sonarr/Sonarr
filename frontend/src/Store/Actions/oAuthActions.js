import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import oAuthActionHandlers from './oAuthActionHandlers';

export const startOAuth = oAuthActionHandlers[types.START_OAUTH];
export const setOAuthValue = createAction(types.SET_OAUTH_VALUE);
export const resetOAuth = createAction(types.RESET_OAUTH);
