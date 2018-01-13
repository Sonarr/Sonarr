import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import selectProviderSchema from 'Utilities/State/selectProviderSchema';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createSetProviderFieldValueReducer from 'Store/Actions/Creators/Reducers/createSetProviderFieldValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createSaveProviderHandler, { createCancelSaveProviderHandler } from 'Store/Actions/Creators/createSaveProviderHandler';
import createTestProviderHandler, { createCancelTestProviderHandler } from 'Store/Actions/Creators/createTestProviderHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';

//
// Variables

const section = 'settings.downloadClients';

//
// Actions Types

export const FETCH_DOWNLOAD_CLIENTS = 'settings/downloadClients/fetchDownloadClients';
export const FETCH_DOWNLOAD_CLIENT_SCHEMA = 'settings/downloadClients/fetchDownloadClientSchema';
export const SELECT_DOWNLOAD_CLIENT_SCHEMA = 'settings/downloadClients/selectDownloadClientSchema';
export const SET_DOWNLOAD_CLIENT_VALUE = 'settings/downloadClients/setDownloadClientValue';
export const SET_DOWNLOAD_CLIENT_FIELD_VALUE = 'settings/downloadClients/setDownloadClientFieldValue';
export const SAVE_DOWNLOAD_CLIENT = 'settings/downloadClients/saveDownloadClient';
export const CANCEL_SAVE_DOWNLOAD_CLIENT = 'settings/downloadClients/cancelSaveDownloadClient';
export const DELETE_DOWNLOAD_CLIENT = 'settings/downloadClients/deleteDownloadClient';
export const TEST_DOWNLOAD_CLIENT = 'settings/downloadClients/testDownloadClient';
export const CANCEL_TEST_DOWNLOAD_CLIENT = 'settings/downloadClients/cancelTestDownloadClient';

//
// Action Creators

export const fetchDownloadClients = createThunk(FETCH_DOWNLOAD_CLIENTS);
export const fetchDownloadClientSchema = createThunk(FETCH_DOWNLOAD_CLIENT_SCHEMA);
export const selectDownloadClientSchema = createAction(SELECT_DOWNLOAD_CLIENT_SCHEMA);

export const saveDownloadClient = createThunk(SAVE_DOWNLOAD_CLIENT);
export const cancelSaveDownloadClient = createThunk(CANCEL_SAVE_DOWNLOAD_CLIENT);
export const deleteDownloadClient = createThunk(DELETE_DOWNLOAD_CLIENT);
export const testDownloadClient = createThunk(TEST_DOWNLOAD_CLIENT);
export const cancelTestDownloadClient = createThunk(CANCEL_TEST_DOWNLOAD_CLIENT);

export const setDownloadClientValue = createAction(SET_DOWNLOAD_CLIENT_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setDownloadClientFieldValue = createAction(SET_DOWNLOAD_CLIENT_FIELD_VALUE, (payload) => {
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
    isFetchingSchema: false,
    isSchemaPopulated: false,
    schemaError: null,
    schema: [],
    selectedSchema: {},
    isSaving: false,
    saveError: null,
    isTesting: false,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_DOWNLOAD_CLIENTS]: createFetchHandler(section, '/downloadclient'),
    [FETCH_DOWNLOAD_CLIENT_SCHEMA]: createFetchSchemaHandler(section, '/downloadclient/schema'),

    [SAVE_DOWNLOAD_CLIENT]: createSaveProviderHandler(section, '/downloadclient'),
    [CANCEL_SAVE_DOWNLOAD_CLIENT]: createCancelSaveProviderHandler(section),
    [DELETE_DOWNLOAD_CLIENT]: createRemoveItemHandler(section, '/downloadclient'),
    [TEST_DOWNLOAD_CLIENT]: createTestProviderHandler(section, '/downloadclient'),
    [CANCEL_TEST_DOWNLOAD_CLIENT]: createCancelTestProviderHandler(section)
  },

  //
  // Reducers

  reducers: {
    [SET_DOWNLOAD_CLIENT_VALUE]: createSetSettingValueReducer(section),
    [SET_DOWNLOAD_CLIENT_FIELD_VALUE]: createSetProviderFieldValueReducer(section),

    [SELECT_DOWNLOAD_CLIENT_SCHEMA]: (state, { payload }) => {
      return selectProviderSchema(state, section, payload, (selectedSchema) => {
        selectedSchema.enable = true;

        return selectedSchema;
      });
    }
  }

};
