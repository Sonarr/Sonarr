import $ from 'jquery';
import _ from 'lodash';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getProviderState from 'Utilities/State/getProviderState';
import { set } from '../baseActions';

const abortCurrentRequests = {};
let lastTestData = null;

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

    const {
      queryParams = {},
      ...otherPayload
    } = payload;

    const testData = getProviderState({ ...otherPayload }, getState, section);
    const params = { ...queryParams };

    // If the user is re-testing the same provider without changes
    // force it to be tested.

    if (_.isEqual(testData, lastTestData)) {
      params.forceTest = true;
    }

    lastTestData = testData;

    const ajaxOptions = {
      url: `${url}/test?${$.param(params, true)}`,
      method: 'POST',
      contentType: 'application/json',
      dataType: 'json',
      data: JSON.stringify(testData)
    };

    const { request, abortRequest } = createAjaxRequest(ajaxOptions);

    abortCurrentRequests[section] = abortRequest;

    request.done((data) => {
      lastTestData = null;

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
