import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { removeItem, set } from '../baseActions';

function createBulkRemoveItemHandler(section, url) {
  return function(getState, payload, dispatch) {
    const {
      ids
    } = payload;

    dispatch(set({ section, isDeleting: true }));

    const ajaxOptions = {
      url: `${url}`,
      method: 'DELETE',
      data: JSON.stringify(payload),
      dataType: 'json'
    };

    const promise = createAjaxRequest(ajaxOptions).request;

    promise.done((data) => {
      dispatch(batchActions([
        set({
          section,
          isDeleting: false,
          deleteError: null
        }),

        ...ids.map((id) => {
          return removeItem({ section, id });
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
}

export default createBulkRemoveItemHandler;
