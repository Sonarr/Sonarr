import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';

//
// Variables

export const section = 'users';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],

  details: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  }
};

//
// Actions Types

export const FETCH_USERS = 'users/fetchUsers';
export const DELETE_USER = 'users/deleteUser';
export const FETCH_USER_DETAILS = 'users/fetchUserDetails';

//
// Action Creators

export const fetchUsers = createThunk(FETCH_USERS);
export const deleteUser = createThunk(DELETE_USER);
export const fetchUserDetails = createThunk(FETCH_USER_DETAILS);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_USERS]: createFetchHandler(section, '/user'),
  [DELETE_USER]: createRemoveItemHandler(section, '/user'),
  [FETCH_USER_DETAILS]: createFetchHandler('users.details', '/user/detail')
});

//
// Reducers
export const reducers = createHandleActions({}, defaultState, section);
