import $ from 'jquery';
import _ from 'lodash';

function flattenProviderData(providerData) {
  return _.reduce(Object.keys(providerData), (result, key) => {
    const property = providerData[key];

    if (key === 'fields') {
      result[key] = property;
    } else {
      result[key] = property.value;
    }

    return result;
  }, {});
}

function requestAction(payload) {
  const {
    provider,
    action,
    providerData,
    queryParams
  } = payload;

  const ajaxOptions = {
    url: `/${provider}/action/${action}`,
    contentType: 'application/json',
    method: 'POST',
    data: JSON.stringify(flattenProviderData(providerData))
  };

  if (queryParams) {
    ajaxOptions.url += `?${$.param(queryParams, true)}`;
  }

  return $.ajax(ajaxOptions);
}

export default requestAction;
