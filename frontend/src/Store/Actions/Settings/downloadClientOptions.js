import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';

//
// Variables

const section = 'settings.downloadClientOptions';

//
// Actions Types

export const FETCH_DOWNLOAD_CLIENT_OPTIONS = 'FETCH_DOWNLOAD_CLIENT_OPTIONS';
export const SET_DOWNLOAD_CLIENT_OPTIONS_VALUE = 'SET_DOWNLOAD_CLIENT_OPTIONS_VALUE';
export const SAVE_DOWNLOAD_CLIENT_OPTIONS = 'SAVE_DOWNLOAD_CLIENT_OPTIONS';

//
// Action Creators

export const fetchDownloadClientOptions = createThunk(FETCH_DOWNLOAD_CLIENT_OPTIONS);
export const saveDownloadClientOptions = createThunk(SAVE_DOWNLOAD_CLIENT_OPTIONS);
export const setDownloadClientOptionsValue = createAction(SET_DOWNLOAD_CLIENT_OPTIONS_VALUE, (payload) => {
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
    [FETCH_DOWNLOAD_CLIENT_OPTIONS]: createFetchHandler(section, '/config/downloadclient'),
    [SAVE_DOWNLOAD_CLIENT_OPTIONS]: createSaveHandler(section, '/config/downloadclient')
  },

  //
  // Reducers

  reducers: {
    [SET_DOWNLOAD_CLIENT_OPTIONS_VALUE]: createSetSettingValueReducer(section)
  }

};
