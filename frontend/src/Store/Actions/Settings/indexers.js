import { createAction } from 'redux-actions';
import createBulkEditItemHandler from 'Store/Actions/Creators/createBulkEditItemHandler';
import createBulkRemoveItemHandler from 'Store/Actions/Creators/createBulkRemoveItemHandler';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler, { createCancelSaveProviderHandler } from 'Store/Actions/Creators/createSaveProviderHandler';
import createTestAllProvidersHandler from 'Store/Actions/Creators/createTestAllProvidersHandler';
import createTestProviderHandler, { createCancelTestProviderHandler } from 'Store/Actions/Creators/createTestProviderHandler';
import createSetProviderFieldValueReducer from 'Store/Actions/Creators/Reducers/createSetProviderFieldValueReducer';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import getSectionState from 'Utilities/State/getSectionState';
import selectProviderSchema from 'Utilities/State/selectProviderSchema';
import updateSectionState from 'Utilities/State/updateSectionState';

//
// Variables

const section = 'settings.indexers';

//
// Actions Types

export const FETCH_INDEXERS = 'settings/indexers/fetchIndexers';
export const FETCH_INDEXER_SCHEMA = 'settings/indexers/fetchIndexerSchema';
export const SELECT_INDEXER_SCHEMA = 'settings/indexers/selectIndexerSchema';
export const CLONE_INDEXER = 'settings/indexers/cloneIndexer';
export const SET_INDEXER_VALUE = 'settings/indexers/setIndexerValue';
export const SET_INDEXER_FIELD_VALUE = 'settings/indexers/setIndexerFieldValue';
export const SAVE_INDEXER = 'settings/indexers/saveIndexer';
export const CANCEL_SAVE_INDEXER = 'settings/indexers/cancelSaveIndexer';
export const DELETE_INDEXER = 'settings/indexers/deleteIndexer';
export const TEST_INDEXER = 'settings/indexers/testIndexer';
export const CANCEL_TEST_INDEXER = 'settings/indexers/cancelTestIndexer';
export const TEST_ALL_INDEXERS = 'settings/indexers/testAllIndexers';
export const BULK_EDIT_INDEXERS = 'settings/indexers/bulkEditIndexers';
export const BULK_DELETE_INDEXERS = 'settings/indexers/bulkDeleteIndexers';

//
// Action Creators

export const fetchIndexers = createThunk(FETCH_INDEXERS);
export const fetchIndexerSchema = createThunk(FETCH_INDEXER_SCHEMA);
export const selectIndexerSchema = createAction(SELECT_INDEXER_SCHEMA);
export const cloneIndexer = createAction(CLONE_INDEXER);

export const saveIndexer = createThunk(SAVE_INDEXER);
export const cancelSaveIndexer = createThunk(CANCEL_SAVE_INDEXER);
export const deleteIndexer = createThunk(DELETE_INDEXER);
export const testIndexer = createThunk(TEST_INDEXER);
export const cancelTestIndexer = createThunk(CANCEL_TEST_INDEXER);
export const testAllIndexers = createThunk(TEST_ALL_INDEXERS);
export const bulkEditIndexers = createThunk(BULK_EDIT_INDEXERS);
export const bulkDeleteIndexers = createThunk(BULK_DELETE_INDEXERS);

export const setIndexerValue = createAction(SET_INDEXER_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setIndexerFieldValue = createAction(SET_INDEXER_FIELD_VALUE, (payload) => {
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
    isDeleting: false,
    deleteError: null,
    isTesting: false,
    isTestingAll: false,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_INDEXERS]: createFetchHandler(section, '/indexer'),
    [FETCH_INDEXER_SCHEMA]: createFetchSchemaHandler(section, '/indexer/schema'),

    [SAVE_INDEXER]: createSaveProviderHandler(section, '/indexer'),
    [CANCEL_SAVE_INDEXER]: createCancelSaveProviderHandler(section),
    [DELETE_INDEXER]: createRemoveItemHandler(section, '/indexer'),
    [TEST_INDEXER]: createTestProviderHandler(section, '/indexer'),
    [CANCEL_TEST_INDEXER]: createCancelTestProviderHandler(section),
    [TEST_ALL_INDEXERS]: createTestAllProvidersHandler(section, '/indexer'),
    [BULK_EDIT_INDEXERS]: createBulkEditItemHandler(section, '/indexer/bulk'),
    [BULK_DELETE_INDEXERS]: createBulkRemoveItemHandler(section, '/indexer/bulk')
  },

  //
  // Reducers

  reducers: {
    [SET_INDEXER_VALUE]: createSetSettingValueReducer(section),
    [SET_INDEXER_FIELD_VALUE]: createSetProviderFieldValueReducer(section),

    [SELECT_INDEXER_SCHEMA]: (state, { payload }) => {
      return selectProviderSchema(state, section, payload, (selectedSchema) => {
        selectedSchema.enableRss = selectedSchema.supportsRss;
        selectedSchema.enableAutomaticSearch = selectedSchema.supportsSearch;
        selectedSchema.enableInteractiveSearch = selectedSchema.supportsSearch;

        return selectedSchema;
      });
    },

    [CLONE_INDEXER]: function(state, { payload }) {
      const id = payload.id;
      const newState = getSectionState(state, section);
      const item = newState.items.find((i) => i.id === id);

      // Use selectedSchema so `createProviderSettingsSelector` works properly
      const selectedSchema = { ...item };
      delete selectedSchema.id;
      delete selectedSchema.name;

      selectedSchema.fields = selectedSchema.fields.map((field) => {
        return { ...field };
      });

      newState.selectedSchema = selectedSchema;

      // Set the name in pendingChanges
      newState.pendingChanges = {
        name: `${item.name} - Copy`
      };

      return updateSectionState(state, section, newState);
    }
  }

};
