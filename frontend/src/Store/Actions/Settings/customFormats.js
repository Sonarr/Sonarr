import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import createBulkEditItemHandler from 'Store/Actions/Creators/createBulkEditItemHandler';
import createBulkRemoveItemHandler from 'Store/Actions/Creators/createBulkRemoveItemHandler';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetClientSideCollectionSortReducer
  from 'Store/Actions/Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import translate from 'Utilities/String/translate';
import { set } from '../baseActions';

//
// Variables

const section = 'settings.customFormats';

//
// Actions Types

export const FETCH_CUSTOM_FORMATS = 'settings/customFormats/fetchCustomFormats';
export const SAVE_CUSTOM_FORMAT = 'settings/customFormats/saveCustomFormat';
export const DELETE_CUSTOM_FORMAT = 'settings/customFormats/deleteCustomFormat';
export const SET_CUSTOM_FORMAT_VALUE = 'settings/customFormats/setCustomFormatValue';
export const CLONE_CUSTOM_FORMAT = 'settings/customFormats/cloneCustomFormat';
export const BULK_EDIT_CUSTOM_FORMATS = 'settings/downloadClients/bulkEditCustomFormats';
export const BULK_DELETE_CUSTOM_FORMATS = 'settings/downloadClients/bulkDeleteCustomFormats';
export const SET_MANAGE_CUSTOM_FORMATS_SORT = 'settings/downloadClients/setManageCustomFormatsSort';

//
// Action Creators

export const fetchCustomFormats = createThunk(FETCH_CUSTOM_FORMATS);
export const saveCustomFormat = createThunk(SAVE_CUSTOM_FORMAT);
export const deleteCustomFormat = createThunk(DELETE_CUSTOM_FORMAT);
export const bulkEditCustomFormats = createThunk(BULK_EDIT_CUSTOM_FORMATS);
export const bulkDeleteCustomFormats = createThunk(BULK_DELETE_CUSTOM_FORMATS);
export const setManageCustomFormatsSort = createAction(SET_MANAGE_CUSTOM_FORMATS_SORT);

export const setCustomFormatValue = createAction(SET_CUSTOM_FORMAT_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const cloneCustomFormat = createAction(CLONE_CUSTOM_FORMAT);

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isSaving: false,
    saveError: null,
    isDeleting: false,
    deleteError: null,
    items: [],
    pendingChanges: {},

    isSchemaFetching: false,
    isSchemaPopulated: false,
    schemaError: null,
    schema: {
      includeCustomFormatWhenRenaming: false
    },

    sortKey: 'name',
    sortDirection: sortDirections.ASCENDING,
    sortPredicates: {
      name: ({ name }) => {
        return name.toLocaleLowerCase();
      }
    }
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_CUSTOM_FORMATS]: createFetchHandler(section, '/customformat'),

    [DELETE_CUSTOM_FORMAT]: createRemoveItemHandler(section, '/customformat'),

    [SAVE_CUSTOM_FORMAT]: (getState, payload, dispatch) => {
      // move the format tags in as a pending change
      const state = getState();
      const pendingChanges = state.settings.customFormats.pendingChanges;
      pendingChanges.specifications = state.settings.customFormatSpecifications.items;
      dispatch(set({
        section,
        pendingChanges
      }));

      createSaveProviderHandler(section, '/customformat')(getState, payload, dispatch);
    },

    [BULK_EDIT_CUSTOM_FORMATS]: createBulkEditItemHandler(section, '/customformat/bulk'),
    [BULK_DELETE_CUSTOM_FORMATS]: createBulkRemoveItemHandler(section, '/customformat/bulk')
  },

  //
  // Reducers

  reducers: {
    [SET_CUSTOM_FORMAT_VALUE]: createSetSettingValueReducer(section),

    [CLONE_CUSTOM_FORMAT]: function(state, { payload }) {
      const id = payload.id;
      const newState = getSectionState(state, section);
      const item = newState.items.find((i) => i.id === id);
      const pendingChanges = { ...item, id: 0 };
      delete pendingChanges.id;

      pendingChanges.name = translate('DefaultNameCopiedProfile', { name: pendingChanges.name });
      newState.pendingChanges = pendingChanges;

      return updateSectionState(state, section, newState);
    },

    [SET_MANAGE_CUSTOM_FORMATS_SORT]: createSetClientSideCollectionSortReducer(section)
  }

};
