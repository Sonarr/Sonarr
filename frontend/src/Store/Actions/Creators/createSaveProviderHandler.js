import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getProviderState from 'Utilities/State/getProviderState';
import { set, updateItem } from '../baseActions';

const abortCurrentRequests = {};

export function createCancelSaveProviderHandler(section) {
  return function(payload) {
    return function(dispatch, getState) {
      if (abortCurrentRequests[section]) {
        abortCurrentRequests[section]();
        abortCurrentRequests[section] = null;
      }
    };
  };
}

function createSaveProviderHandler(section, url, getFromState) {
  return function(payload) {
    return function(dispatch, getState) {
      dispatch(set({ section, isSaving: true }));

      const id = payload.id;
      const saveData = getProviderState(payload, getState, getFromState);

      const ajaxOptions = {
        url,
        method: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(saveData)
      };

      if (id) {
        ajaxOptions.url = `${url}/${id}`;
        ajaxOptions.method = 'PUT';
      }

      const { request, abortRequest } = createAjaxRequest()(ajaxOptions);

      abortCurrentRequests[section] = abortRequest;

      request.done((data) => {
        dispatch(batchActions([
          updateItem({ section, ...data }),

          set({
            section,
            isSaving: false,
            saveError: null,
            pendingChanges: {}
          })
        ]));
      });

      request.fail((xhr) => {
        dispatch(set({
          section,
          isSaving: false,
          saveError: xhr.aborted ? null : xhr
        }));
      });
    };
  };
}

export default createSaveProviderHandler;
