import $ from 'jquery';

const absUrlRegex = /^(https?:)?\/\//i;
const apiRoot = window.Sonarr.apiRoot;

function isRelative(ajaxOptions) {
  return !absUrlRegex.test(ajaxOptions.url);
}

function moveBodyToQuery(ajaxOptions) {
  if (ajaxOptions.data && ajaxOptions.type === 'DELETE') {
    if (ajaxOptions.url.contains('?')) {
      ajaxOptions.url += '&';
    } else {
      ajaxOptions.url += '?';
    }
    ajaxOptions.url += $.param(ajaxOptions.data);
    delete ajaxOptions.data;
  }
}

function addRootUrl(ajaxOptions) {
  ajaxOptions.url = apiRoot + ajaxOptions.url;
}

function addApiKey(ajaxOptions) {
  ajaxOptions.headers = ajaxOptions.headers || {};
  ajaxOptions.headers['X-Api-Key'] = window.Sonarr.apiKey;
}

export default function createAjaxRequest(originalAjaxOptions) {
  const requestXHR = new window.XMLHttpRequest();
  let aborted = false;
  let complete = false;

  function abortRequest() {
    if (!complete) {
      aborted = true;
      requestXHR.abort();
    }
  }

  const ajaxOptions = { dataType: 'json', ...originalAjaxOptions };

  if (isRelative(ajaxOptions)) {
    moveBodyToQuery(ajaxOptions);
    addRootUrl(ajaxOptions);
    addApiKey(ajaxOptions);
  }

  const request = $.ajax({
    xhr: () => requestXHR,
    ...ajaxOptions
  }).then(null, (xhr, textStatus, errorThrown) => {
    xhr.aborted = aborted;

    return $.Deferred().reject(xhr, textStatus, errorThrown).promise();
  }).always(() => {
    complete = true;
  });

  return {
    request,
    abortRequest
  };
}
