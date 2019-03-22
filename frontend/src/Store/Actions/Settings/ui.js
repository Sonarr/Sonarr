import { createAction } from 'redux-actions';
import { createThunk } from 'Store/thunks';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import createFetchHandler from 'Store/Actions/Creators/createFetchHandler';
import createSaveHandler from 'Store/Actions/Creators/createSaveHandler';

//
// Variables

const section = 'settings.ui';

//
// Actions Types

export const FETCH_UI_SETTINGS = 'settings/ui/fetchUiSettings';
export const SET_UI_SETTINGS_VALUE = 'SET_UI_SETTINGS_VALUE';
export const SAVE_UI_SETTINGS = 'SAVE_UI_SETTINGS';

//
// Action Creators

export const fetchUISettings = createThunk(FETCH_UI_SETTINGS);
export const saveUISettings = createThunk(SAVE_UI_SETTINGS);
export const setUISettingsValue = createAction(SET_UI_SETTINGS_VALUE, (payload) => {
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
    [FETCH_UI_SETTINGS]: createFetchHandler(section, '/config/ui'),
    [SAVE_UI_SETTINGS]: createSaveHandler(section, '/config/ui')
  },

  //
  // Reducers

  reducers: {
    [SET_UI_SETTINGS_VALUE]: createSetSettingValueReducer(section)
  }

};
