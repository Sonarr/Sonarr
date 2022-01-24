import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
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

//
// Action Creators

export const fetchCustomFormats = createThunk(FETCH_CUSTOM_FORMATS);
export const saveCustomFormat = createThunk(SAVE_CUSTOM_FORMAT);
export const deleteCustomFormat = createThunk(DELETE_CUSTOM_FORMAT);

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
    isSchemaFetching: false,
    isSchemaPopulated: false,
    isFetching: false,
    isPopulated: false,
    schema: {
      includeCustomFormatWhenRenaming: false
    },
    error: null,
    isDeleting: false,
    deleteError: null,
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
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
    }
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

      pendingChanges.name = `${pendingChanges.name} - Copy`;
      newState.pendingChanges = pendingChanges;

      return updateSectionState(state, section, newState);
    }
  }

};
