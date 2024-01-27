import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.importListOptions';

//
// Actions Types

export const FETCH_IMPORT_LIST_OPTIONS = 'settings/importListOptions/fetchImportListOptions';
export const SAVE_IMPORT_LIST_OPTIONS = 'settings/importListOptions/saveImportListOptions';
export const SET_IMPORT_LIST_OPTIONS_VALUE = 'settings/importListOptions/setImportListOptionsValue';

//
// Action Creators

export const fetchImportListOptions = createThunk(FETCH_IMPORT_LIST_OPTIONS);
export const saveImportListOptions = createThunk(SAVE_IMPORT_LIST_OPTIONS);
export const setImportListOptionsValue = createAction(SET_IMPORT_LIST_OPTIONS_VALUE, (payload) => {
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
    [FETCH_IMPORT_LIST_OPTIONS]: createFetchHandler(section, '/config/importlist'),
    [SAVE_IMPORT_LIST_OPTIONS]: createSaveHandler(section, '/config/importlist')
  },

  //
  // Reducers

  reducers: {
    [SET_IMPORT_LIST_OPTIONS_VALUE]: createSetSettingValueReducer(section)
  }

};
