import $ from 'jquery';
import { batchActions } from 'redux-batched-actions';
import { set, removeItem } from '../baseActions';

function createRemoveItemHandler(section, url) {
  return function(payload) {
    return function(dispatch, getState) {
      const {
        id,
        ...queryParms
      } = payload;

      dispatch(set({ section, isDeleting: true }));

      const ajaxOptions = {
        url: `${url}/${id}?${$.param(queryParms, true)}`,
        method: 'DELETE'
      };

      const promise = $.ajax(ajaxOptions);

      promise.done((data) => {
        dispatch(batchActions([
          removeItem({ section, id }),

          set({
            section,
            isDeleting: false,
            deleteError: null
          })
        ]));
      });

      promise.fail((xhr) => {
        dispatch(set({
          section,
          isDeleting: false,
          deleteError: xhr
        }));
      });

      return promise;
    };
  };
}

export default createRemoveItemHandler;
