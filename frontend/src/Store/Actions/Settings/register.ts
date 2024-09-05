import { createAction } from 'redux-actions';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import { InputChanged } from 'typings/inputs';
import { set } from '../baseActions';
// import createCustomFetchRegister from '../Creators/createCustomFetchRegister';
import createSaveProviderHandler from '../Creators/createSaveProviderHandler';

//
// Variables

export const section = 'settings.register';

//
// Action Types

export const FETCH_REGISTER_SETTINGS =
  'settings/register/fetchRegisterSettings';
export const SET_REGISTER_SETTINGS_VALUE =
  'settings/register/setRegisterSettingsValue';
export const SAVE_REGISTER_SETTINGS = 'settings/register/saveRegisterSettings';

//
// Action Creators

export const fetchRegisterSettings = createThunk(FETCH_REGISTER_SETTINGS);
export const saveRegisterSettings = createThunk(SAVE_REGISTER_SETTINGS);

export const setRegisterValue = createAction(
  SET_REGISTER_SETTINGS_VALUE,
  (payload: InputChanged) => {
    return {
      section,
      ...payload,
    };
  }
);

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
    item: {},
  },

  //
  // Action Handlers

  actionHandlers: {
    // [FETCH_REGISTER_SETTINGS]: createCustomFetchRegister(section),
    [SAVE_REGISTER_SETTINGS]: (getState, payload, dispatch) => {
      const state = getState();
      const pendingChanges = state.settings.register.pendingChanges;

      dispatch(
        set({
          section,
          pendingChanges,
        })
      );

      createSaveProviderHandler(section, '/user')(getState, payload, dispatch);
    },
  },

  //
  // Reducers

  reducers: {
    [SET_REGISTER_SETTINGS_VALUE]: createSetSettingValueReducer(section),
  },
};
