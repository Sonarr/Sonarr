import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import seriesEditorActionHandlers from './seriesEditorActionHandlers';

export const setSeriesEditorSort = createAction(types.SET_SERIES_EDITOR_SORT);
export const setSeriesEditorFilter = createAction(types.SET_SERIES_EDITOR_FILTER);
export const saveSeriesEditor = seriesEditorActionHandlers[types.SAVE_SERIES_EDITOR];
export const bulkDeleteSeries = seriesEditorActionHandlers[types.BULK_DELETE_SERIES];
