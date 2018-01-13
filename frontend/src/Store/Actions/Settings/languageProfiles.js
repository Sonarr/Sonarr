import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createFetchSchemaHandler from 'Store/Actions/Creators/createFetchSchemaHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';

//
// Variables

const section = 'settings.languageProfiles';

//
// Actions Types

export const FETCH_LANGUAGE_PROFILES = 'settings/languageProfiles/fetchLanguageProfiles';
export const FETCH_LANGUAGE_PROFILE_SCHEMA = 'settings/languageProfiles/fetchLanguageProfileSchema';
export const SAVE_LANGUAGE_PROFILE = 'settings/languageProfiles/saveLanguageProfile';
export const DELETE_LANGUAGE_PROFILE = 'settings/languageProfiles/deleteLanguageProfile';
export const SET_LANGUAGE_PROFILE_VALUE = 'settings/languageProfiles/setLanguageProfileValue';
export const CLONE_LANGUAGE_PROFILE = 'settings/languageProfiles/cloneLanguageProfile';

//
// Action Creators

export const fetchLanguageProfiles = createThunk(FETCH_LANGUAGE_PROFILES);
export const fetchLanguageProfileSchema = createThunk(FETCH_LANGUAGE_PROFILE_SCHEMA);
export const saveLanguageProfile = createThunk(SAVE_LANGUAGE_PROFILE);
export const deleteLanguageProfile = createThunk(DELETE_LANGUAGE_PROFILE);

export const setLanguageProfileValue = createAction(SET_LANGUAGE_PROFILE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const cloneLanguageProfile = createAction(CLONE_LANGUAGE_PROFILE);

//
// Details

export default {

  //
  // State

  defaultState: {
    isFetching: false,
    isPopulated: false,
    error: null,
    isDeleting: false,
    deleteError: null,
    isFetchingSchema: false,
    isSchemaPopulated: false,
    schemaError: null,
    schema: {},
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_LANGUAGE_PROFILES]: createFetchHandler(section, '/languageprofile'),
    [FETCH_LANGUAGE_PROFILE_SCHEMA]: createFetchSchemaHandler(section, '/languageprofile/schema'),
    [SAVE_LANGUAGE_PROFILE]: createSaveProviderHandler(section, '/languageprofile'),
    [DELETE_LANGUAGE_PROFILE]: createRemoveItemHandler(section, '/languageprofile')
  },

  //
  // Reducers

  reducers: {
    [SET_LANGUAGE_PROFILE_VALUE]: createSetSettingValueReducer(section),

    [CLONE_LANGUAGE_PROFILE]: function(state, { payload }) {
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
