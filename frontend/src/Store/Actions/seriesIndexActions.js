import { createAction } from 'redux-actions';
import * as types from './actionTypes';

export const setSeriesSort = createAction(types.SET_SERIES_SORT);
export const setSeriesFilter = createAction(types.SET_SERIES_FILTER);
export const setSeriesView = createAction(types.SET_SERIES_VIEW);
export const setSeriesTableOption = createAction(types.SET_SERIES_TABLE_OPTION);
export const setSeriesPosterOption = createAction(types.SET_SERIES_POSTER_OPTION);
export const setSeriesOverviewOption = createAction(types.SET_SERIES_OVERVIEW_OPTION);
