import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import addSeriesActionHandlers from './addSeriesActionHandlers';

export const lookupSeries = addSeriesActionHandlers[types.LOOKUP_SERIES];
export const addSeries = addSeriesActionHandlers[types.ADD_SERIES];
export const clearAddSeries = createAction(types.CLEAR_ADD_SERIES);
export const setAddSeriesDefault = createAction(types.SET_ADD_SERIES_DEFAULT);

export const setAddSeriesValue = createAction(types.SET_ADD_SERIES_VALUE, (payload) => {
  return {
    section: 'addSeries',
    ...payload
  };
});
