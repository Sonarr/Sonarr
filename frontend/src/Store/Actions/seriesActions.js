import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import seriesActionHandlers from './seriesActionHandlers';

export const fetchSeries = seriesActionHandlers[types.FETCH_SERIES];
export const saveSeries = seriesActionHandlers[types.SAVE_SERIES];
export const deleteSeries = seriesActionHandlers[types.DELETE_SERIES];
export const toggleSeriesMonitored = seriesActionHandlers[types.TOGGLE_SERIES_MONITORED];
export const toggleSeasonMonitored = seriesActionHandlers[types.TOGGLE_SEASON_MONITORED];

export const setSeriesValue = createAction(types.SET_SERIES_VALUE, (payload) => {
  return {
    section: 'series',
    ...payload
  };
});
