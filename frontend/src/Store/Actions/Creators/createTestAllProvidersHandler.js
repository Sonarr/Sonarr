import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set } from '../baseActions';

function createTestAllProvidersHandler(section, url) {
  return function(getState, payload, dispatch) {
    dispatch(set({ section, isTestingAll: true }));

    const ajaxOptions = {
      url: `${url}/testall`,
      method: 'POST',
      contentType: 'application/json',
      dataType: 'json'
    };

    const { request } = createAjaxRequest(ajaxOptions);

    request.done((data) => {
      dispatch(set({
        section,
        isTestingAll: false,
        saveError: null
      }));
    });

    request.fail((xhr) => {
      dispatch(set({
        section,
        isTestingAll: false
      }));
    });
  };
}

export default createTestAllProvidersHandler;
