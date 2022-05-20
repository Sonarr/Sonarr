import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler, { createCancelSaveProviderHandler } from 'Store/Actions/Creators/createSaveProviderHandler';
import createTestAllProvidersHandler from 'Store/Actions/Creators/createTestAllProvidersHandler';
import createTestProviderHandler, { createCancelTestProviderHandler } from 'Store/Actions/Creators/createTestProviderHandler';
import createSetProviderFieldValueReducer from 'Store/Actions/Creators/Reducers/createSetProviderFieldValueReducer';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import selectProviderSchema from 'Utilities/State/selectProviderSchema';

//
// Variables

const section = 'settings.importLists';

//
// Actions Types

export const FETCH_IMPORT_LISTS = 'settings/importlists/fetchImportLists';
export const FETCH_IMPORT_LIST_SCHEMA = 'settings/importlists/fetchImportListSchema';
export const SELECT_IMPORT_LIST_SCHEMA = 'settings/importlists/selectImportListSchema';
export const SET_IMPORT_LIST_VALUE = 'settings/importlists/setImportListValue';
export const SET_IMPORT_LIST_FIELD_VALUE = 'settings/importlists/setImportListFieldValue';
export const SAVE_IMPORT_LIST = 'settings/importlists/saveImportList';
export const CANCEL_SAVE_IMPORT_LIST = 'settings/importlists/cancelSaveImportList';
export const DELETE_IMPORT_LIST = 'settings/importlists/deleteImportList';
export const TEST_IMPORT_LIST = 'settings/importlists/testImportList';
export const CANCEL_TEST_IMPORT_LIST = 'settings/importlists/cancelTestImportList';
export const TEST_ALL_IMPORT_LISTS = 'settings/importlists/testAllImportLists';

//
// Action Creators

export const fetchImportLists = createThunk(FETCH_IMPORT_LISTS);
export const fetchImportListSchema = createThunk(FETCH_IMPORT_LIST_SCHEMA);
export const selectImportListSchema = createAction(SELECT_IMPORT_LIST_SCHEMA);

export const saveImportList = createThunk(SAVE_IMPORT_LIST);
export const cancelSaveImportList = createThunk(CANCEL_SAVE_IMPORT_LIST);
export const deleteImportList = createThunk(DELETE_IMPORT_LIST);
export const testImportList = createThunk(TEST_IMPORT_LIST);
export const cancelTestImportList = createThunk(CANCEL_TEST_IMPORT_LIST);
export const testAllImportLists = createThunk(TEST_ALL_IMPORT_LISTS);

export const setImportListValue = createAction(SET_IMPORT_LIST_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setImportListFieldValue = createAction(SET_IMPORT_LIST_FIELD_VALUE, (payload) => {
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
    isSchemaFetching: false,
    isSchemaPopulated: false,
    schemaError: null,
    schema: [],
    selectedSchema: {},
    isSaving: false,
    saveError: null,
    isTesting: false,
    isTestingAll: false,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_IMPORT_LISTS]: createFetchHandler(section, '/importlist'),
    [FETCH_IMPORT_LIST_SCHEMA]: createFetchSchemaHandler(section, '/importlist/schema'),
    [SAVE_IMPORT_LIST]: createSaveProviderHandler(section, '/importlist'),
    [CANCEL_SAVE_IMPORT_LIST]: createCancelSaveProviderHandler(section),
    [DELETE_IMPORT_LIST]: createRemoveItemHandler(section, '/importlist'),
    [TEST_IMPORT_LIST]: createTestProviderHandler(section, '/importlist'),
    [CANCEL_TEST_IMPORT_LIST]: createCancelTestProviderHandler(section),
    [TEST_ALL_IMPORT_LISTS]: createTestAllProvidersHandler(section, '/importlist')
  },

  //
  // Reducers

  reducers: {
    [SET_IMPORT_LIST_VALUE]: createSetSettingValueReducer(section),
    [SET_IMPORT_LIST_FIELD_VALUE]: createSetProviderFieldValueReducer(section),

    [SELECT_IMPORT_LIST_SCHEMA]: (state, { payload }) => {
      return selectProviderSchema(state, section, payload, (selectedSchema) => {
        selectedSchema.enableAutomaticAdd = true;
        selectedSchema.shouldMonitor = 'all';
        selectedSchema.seriesType = 'standard';
        selectedSchema.seasonFolder = true;

        return selectedSchema;
      });
    }
  }

};
