import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import { set, updateItem } from './baseActions';

//
// Variables

export const section = 'rootFolders';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  items: []
};

//
// Actions Types

export const FETCH_ROOT_FOLDERS = 'rootFolders/fetchRootFolders';
export const ADD_ROOT_FOLDER = 'rootFolders/addRootFolder';
export const DELETE_ROOT_FOLDER = 'rootFolders/deleteRootFolder';

//
// Action Creators

export const fetchRootFolders = createThunk(FETCH_ROOT_FOLDERS);
export const addRootFolder = createThunk(ADD_ROOT_FOLDER);
export const deleteRootFolder = createThunk(DELETE_ROOT_FOLDER);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_ROOT_FOLDERS]: createFetchHandler('rootFolders', '/rootFolder'),

  [DELETE_ROOT_FOLDER]: createRemoveItemHandler(
    'rootFolders',
    '/rootFolder',
    (state) => state.rootFolders
  ),

  [ADD_ROOT_FOLDER]: function(getState, payload, dispatch) {
    const path = payload.path;

    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = $.ajax({
      url: '/rootFolder',
      method: 'POST',
      data: JSON.stringify({ path }),
      dataType: 'json'
    });

    promise.done((data) => {
      dispatch(batchActions([
        updateItem({
          section,
          ...data
        }),

        set({
          section,
          isSaving: false,
          saveError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  }

});

//
// Reducers

export const reducers = createHandleActions({}, defaultState, section);
