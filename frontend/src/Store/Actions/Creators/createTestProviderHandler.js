import createAjaxRequest from 'Utilities/createAjaxRequest';
import getProviderState from 'Utilities/State/getProviderState';
import { set } from '../baseActions';

const abortCurrentRequests = {};

export function createCancelTestProviderHandler(section) {
  return function(getState, payload, dispatch) {
    if (abortCurrentRequests[section]) {
      abortCurrentRequests[section]();
      abortCurrentRequests[section] = null;
    }
  };
}

function createTestProviderHandler(section, url) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, isTesting: true }));

    const testData = getProviderState(payload, getState, section);

    const ajaxOptions = {
      url: `${url}/test`,
      method: 'POST',
      contentType: 'application/json',
      dataType: 'json',
      data: JSON.stringify(testData)
    };

    const { request, abortRequest } = createAjaxRequest(ajaxOptions);

    abortCurrentRequests[section] = abortRequest;

    request.done((data) => {
      dispatch(set({
        section,
        isTesting: false,
        saveError: null
      }));
    });

    request.fail((xhr) => {
      dispatch(set({
        section,
        isTesting: false,
        saveError: xhr.aborted ? null : xhr
      }));
    });
  };
}

export default createTestProviderHandler;
