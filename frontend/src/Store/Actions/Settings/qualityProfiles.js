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

const section = 'settings.qualityProfiles';

//
// Actions Types

export const FETCH_QUALITY_PROFILES = 'settings/qualityProfiles/fetchQualityProfiles';
export const FETCH_QUALITY_PROFILE_SCHEMA = 'settings/qualityProfiles/fetchQualityProfileSchema';
export const SAVE_QUALITY_PROFILE = 'settings/qualityProfiles/saveQualityProfile';
export const DELETE_QUALITY_PROFILE = 'settings/qualityProfiles/deleteQualityProfile';
export const SET_QUALITY_PROFILE_VALUE = 'settings/qualityProfiles/setQualityProfileValue';
export const CLONE_QUALITY_PROFILE = 'settings/qualityProfiles/cloneQualityProfile';

//
// Action Creators

export const fetchQualityProfiles = createThunk(FETCH_QUALITY_PROFILES);
export const fetchQualityProfileSchema = createThunk(FETCH_QUALITY_PROFILE_SCHEMA);
export const saveQualityProfile = createThunk(SAVE_QUALITY_PROFILE);
export const deleteQualityProfile = createThunk(DELETE_QUALITY_PROFILE);

export const setQualityProfileValue = createAction(SET_QUALITY_PROFILE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const cloneQualityProfile = createAction(CLONE_QUALITY_PROFILE);

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
    [FETCH_QUALITY_PROFILES]: createFetchHandler(section, '/qualityprofile'),
    [FETCH_QUALITY_PROFILE_SCHEMA]: createFetchSchemaHandler(section, '/qualityprofile/schema'),
    [SAVE_QUALITY_PROFILE]: createSaveProviderHandler(section, '/qualityprofile'),
    [DELETE_QUALITY_PROFILE]: createRemoveItemHandler(section, '/qualityprofile')
  },

  //
  // Reducers

  reducers: {
    [SET_QUALITY_PROFILE_VALUE]: createSetSettingValueReducer(section),

    [CLONE_QUALITY_PROFILE]: function(state, { payload }) {
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
