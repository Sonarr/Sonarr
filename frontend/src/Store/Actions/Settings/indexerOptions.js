import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';

//
// Variables

const section = 'settings.indexerOptions';

//
// Actions Types

export const FETCH_INDEXER_OPTIONS = 'settings/indexerOptions/fetchIndexerOptions';
export const SAVE_INDEXER_OPTIONS = 'settings/indexerOptions/saveIndexerOptions';
export const SET_INDEXER_OPTIONS_VALUE = 'settings/indexerOptions/setIndexerOptionsValue';

//
// Action Creators

export const fetchIndexerOptions = createThunk(FETCH_INDEXER_OPTIONS);
export const saveIndexerOptions = createThunk(SAVE_INDEXER_OPTIONS);
export const setIndexerOptionsValue = createAction(SET_INDEXER_OPTIONS_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    pendingChanges: {},
    isSaving: false,
    saveError: null,
    item: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_INDEXER_OPTIONS]: createFetchHandler(section, '/config/indexer'),
    [SAVE_INDEXER_OPTIONS]: createSaveHandler(section, '/config/indexer')
  },

  //
  // Reducers

  reducers: {
    [SET_INDEXER_OPTIONS_VALUE]: createSetSettingValueReducer(section)
  }

};
