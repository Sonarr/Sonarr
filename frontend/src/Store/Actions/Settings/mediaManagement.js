import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';

//
// Variables

const section = 'settings.mediaManagement';

//
// Actions Types

export const FETCH_MEDIA_MANAGEMENT_SETTINGS = 'settings/mediaManagement/fetchMediaManagementSettings';
export const SAVE_MEDIA_MANAGEMENT_SETTINGS = 'settings/mediaManagement/saveMediaManagementSettings';
export const SET_MEDIA_MANAGEMENT_SETTINGS_VALUE = 'settings/mediaManagement/setMediaManagementSettingsValue';

//
// Action Creators

export const fetchMediaManagementSettings = createThunk(FETCH_MEDIA_MANAGEMENT_SETTINGS);
export const saveMediaManagementSettings = createThunk(SAVE_MEDIA_MANAGEMENT_SETTINGS);
export const setMediaManagementSettingsValue = createAction(SET_MEDIA_MANAGEMENT_SETTINGS_VALUE, (payload) => {
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
    [FETCH_MEDIA_MANAGEMENT_SETTINGS]: createFetchHandler(section, '/config/mediamanagement'),
    [SAVE_MEDIA_MANAGEMENT_SETTINGS]: createSaveHandler(section, '/config/mediamanagement')
  },

  //
  // Reducers

  reducers: {
    [SET_MEDIA_MANAGEMENT_SETTINGS_VALUE]: createSetSettingValueReducer(section)
  }

};
