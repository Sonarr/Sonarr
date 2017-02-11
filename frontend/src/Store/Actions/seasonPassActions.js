import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import seasonPassActionHandlers from './seasonPassActionHandlers';

export const setSeasonPassSort = createAction(types.SET_SEASON_PASS_SORT);
export const setSeasonPassFilter = createAction(types.SET_SEASON_PASS_FILTER);
export const saveSeasonPass = seasonPassActionHandlers[types.SAVE_SEASON_PASS];
