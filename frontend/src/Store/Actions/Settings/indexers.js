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

const section = 'settings.indexers';

//
// Actions Types

export const FETCH_INDEXERS = 'settings/indexers/fetchIndexers';
export const FETCH_INDEXER_SCHEMA = 'settings/indexers/fetchIndexerSchema';
export const SELECT_INDEXER_SCHEMA = 'settings/indexers/selectIndexerSchema';
export const SET_INDEXER_VALUE = 'settings/indexers/setIndexerValue';
export const SET_INDEXER_FIELD_VALUE = 'settings/indexers/setIndexerFieldValue';
export const SAVE_INDEXER = 'settings/indexers/saveIndexer';
export const CANCEL_SAVE_INDEXER = 'settings/indexers/cancelSaveIndexer';
export const DELETE_INDEXER = 'settings/indexers/deleteIndexer';
export const TEST_INDEXER = 'settings/indexers/testIndexer';
export const CANCEL_TEST_INDEXER = 'settings/indexers/cancelTestIndexer';

//
// Action Creators

export const fetchIndexers = createThunk(FETCH_INDEXERS);
export const fetchIndexerSchema = createThunk(FETCH_INDEXER_SCHEMA);
export const selectIndexerSchema = createAction(SELECT_INDEXER_SCHEMA);

export const saveIndexer = createThunk(SAVE_INDEXER);
export const cancelSaveIndexer = createThunk(CANCEL_SAVE_INDEXER);
export const deleteIndexer = createThunk(DELETE_INDEXER);
export const testIndexer = createThunk(TEST_INDEXER);
export const cancelTestIndexer = createThunk(CANCEL_TEST_INDEXER);

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
    [FETCH_INDEXERS]: createFetchHandler(section, '/indexer'),
    [FETCH_INDEXER_SCHEMA]: createFetchSchemaHandler(section, '/indexer/schema'),

    [SAVE_INDEXER]: createSaveProviderHandler(section, '/indexer'),
    [CANCEL_SAVE_INDEXER]: createCancelSaveProviderHandler(section),
    [DELETE_INDEXER]: createRemoveItemHandler(section, '/indexer'),
    [TEST_INDEXER]: createTestProviderHandler(section, '/indexer'),
    [CANCEL_TEST_INDEXER]: createCancelTestProviderHandler(section)
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
    }
  }

};
