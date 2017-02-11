import { createAction } from 'redux-actions';
import * as types from './actionTypes';
import importSeriesActionHandlers from './importSeriesActionHandlers';

export const queueLookupSeries = importSeriesActionHandlers[types.QUEUE_LOOKUP_SERIES];
export const startLookupSeries = importSeriesActionHandlers[types.START_LOOKUP_SERIES];
export const importSeries = importSeriesActionHandlers[types.IMPORT_SERIES];
export const clearImportSeries = createAction(types.CLEAR_IMPORT_SERIES);

export const setImportSeriesValue = createAction(types.SET_IMPORT_SERIES_VALUE, (payload) => {
  return {

    section: 'importSeries',
    ...payload
  };
});
