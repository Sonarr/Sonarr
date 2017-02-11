import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import * as types from './actionTypes';
import createFetchHandler from './Creators/createFetchHandler';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import { set, updateItem } from './baseActions';

const section = 'rootFolders';

const rootFolderActionHandlers = {
  [types.FETCH_ROOT_FOLDERS]: createFetchHandler('rootFolders', '/rootFolder'),

  [types.DELETE_ROOT_FOLDER]: createRemoveItemHandler(
    'rootFolders',
    '/rootFolder',
    (state) => state.rootFolders),

  [types.ADD_ROOT_FOLDER]: function(payload) {
    return function(dispatch, getState) {
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
    };
  }
};

export default rootFolderActionHandlers;
