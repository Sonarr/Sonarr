import { createAction } from 'redux-actions';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';

//
// Variables

const section = 'settings.naming';

//
// Actions Types

export const FETCH_NAMING_SETTINGS = 'settings/naming/fetchNamingSettings';
export const SAVE_NAMING_SETTINGS = 'settings/naming/saveNamingSettings';
export const SET_NAMING_SETTINGS_VALUE = 'settings/naming/setNamingSettingsValue';

//
// Action Creators

export const fetchNamingSettings = createThunk(FETCH_NAMING_SETTINGS);
export const saveNamingSettings = createThunk(SAVE_NAMING_SETTINGS);
export const setNamingSettingsValue = createAction(SET_NAMING_SETTINGS_VALUE, (payload) => {
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
    [FETCH_NAMING_SETTINGS]: createFetchHandler(section, '/config/naming'),
    [SAVE_NAMING_SETTINGS]: createSaveHandler(section, '/config/naming')
  },

  //
  // Reducers

  reducers: {
    [SET_NAMING_SETTINGS_VALUE]: createSetSettingValueReducer(section)
  }

};
