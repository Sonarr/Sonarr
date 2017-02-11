import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import pathActionHandlers from './pathActionHandlers';

export const fetchPaths = pathActionHandlers[types.FETCH_PATHS];
export const updatePaths = createAction(types.UPDATE_PATHS);
export const clearPaths = createAction(types.CLEAR_PATHS);
