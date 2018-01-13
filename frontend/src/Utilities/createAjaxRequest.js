import $ from 'jquery';

export default function createAjaxRequest(ajaxOptions) {
  const requestXHR = new window.XMLHttpRequest();
  let aborted = false;
  let complete = false;

  function abortRequest() {
    if (!complete) {
      aborted = true;
      requestXHR.abort();
    }
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
