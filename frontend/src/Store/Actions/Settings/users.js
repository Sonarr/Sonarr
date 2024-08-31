import { createAction } from 'redux-actions';
import { set } from 'Store/Actions/baseActions';
import createRemoveItemHandler from 'Store/Actions/Creators/createRemoveItemHandler';
import createSaveProviderHandler from 'Store/Actions/Creators/createSaveProviderHandler';
import createSetSettingValueReducer from 'Store/Actions/Creators/Reducers/createSetSettingValueReducer';
import { createThunk } from 'Store/thunks';
import createFetchHandler from '../Creators/createFetchHandler';

//
// Variables

const section = 'settings.users';

//
// Actions Types

export const SAVE_USER = 'settings/users/saveUser';
export const DELETE_USER = 'settings/users/deleteUser';
export const SET_USER_VALUE = 'settings/users/setUserValue';

export const FETCH_USERS = 'settings/users/fetchUsers';

//
// Action Creators

export const saveUser = createThunk(SAVE_USER);
export const deleteUser = createThunk(DELETE_USER);

export const setUserValue = createAction(SET_USER_VALUE, (payload) => {
  console.log(payload);
  return {
    section,
    ...payload
  };
});

export const fetchUsers = createThunk(FETCH_USERS);

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
    isSaving: false,
    saveError: null,
    items: [],
    pendingChanges: {}
  },

  //
  // Action Handlers

  actionHandlers: {
    [FETCH_USERS]: createFetchHandler(section, '/user'),
    [DELETE_USER]: createRemoveItemHandler(section, '/user'),
    [SAVE_USER]: (getState, payload, dispatch) => {
      const state = getState();
      const pendingChanges = state.settings.users.pendingChanges;

      dispatch(set({
        section,
        pendingChanges
      }));

      createSaveProviderHandler(section, '/user')(getState, payload, dispatch);
    }
  },

  //
  // Reducers

  reducers: {
    [SET_USER_VALUE]: createSetSettingValueReducer(section)
  }

};
