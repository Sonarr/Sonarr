import { createAction } from 'redux-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

//
// Variables

export const section = 'paths';

//
// State

export const defaultState = {
  currentPath: '',
  isPopulated: false,
  isFetching: false,
  error: null,
  directories: [],
  files: [],
  parent: null
};

//
// Actions Types

export const FETCH_PATHS = 'paths/fetchPaths';
export const UPDATE_PATHS = 'paths/updatePaths';
export const CLEAR_PATHS = 'paths/clearPaths';

//
// Action Creators

export const fetchPaths = createThunk(FETCH_PATHS);
export const updatePaths = createAction(UPDATE_PATHS);
export const clearPaths = createAction(CLEAR_PATHS);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_PATHS]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const {
      path,
      allowFoldersWithoutTrailingSlashes = false,
      includeFiles = false
    } = payload;

    const promise = createAjaxRequest({
      url: '/filesystem',
      data: {
        path,
        allowFoldersWithoutTrailingSlashes,
        includeFiles
      }
    }).request;

    promise.done((data) => {
      dispatch(updatePaths({ path, ...data }));

      dispatch(set({
        section,
        isFetching: false,
        isPopulated: true,
        error: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr
      }));
    });
  }

});

//
// Reducers

export const reducers = createHandleActions({

  [UPDATE_PATHS]: (state, { payload }) => {
    const newState = Object.assign({}, state);

    newState.currentPath = payload.path;
    newState.directories = payload.directories;
    newState.files = payload.files;
    newState.parent = payload.parent;

    return newState;
  },

  [CLEAR_PATHS]: (state, { payload }) => {
    const newState = Object.assign({}, state);

    newState.path = '';
    newState.directories = [];
    newState.files = [];
    newState.parent = '';

    return newState;
  }

}, defaultState, section);
