import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, updateItem } from '../baseActions';

function createBulkEditItemHandler(section, url) {
  return function(getState, payload, dispatch) {

    dispatch(set({ section, isSaving: true }));

    const ajaxOptions = {
      url: `${url}`,
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    };

    const promise = createAjaxRequest(ajaxOptions).request;

    promise.done((data) => {
      dispatch(batchActions([
        set({
          section,
          isSaving: false,
          saveError: null
        }),

        ...data.map((provider) => {

          const {
            ...propsToUpdate
          } = provider;

          return updateItem({
            id: provider.id,
            section,
            ...propsToUpdate
          });
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

    return promise;
  };
}

export default createBulkEditItemHandler;
