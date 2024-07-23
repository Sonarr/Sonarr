interface AjaxResponse {
  responseJSON:
    | {
        message: string | undefined;
      }
    | undefined;
}

function getErrorMessage(xhr: AjaxResponse, fallbackErrorMessage?: string) {
  if (!xhr || !xhr.responseJSON || !xhr.responseJSON.message) {
    return fallbackErrorMessage;
  }

  const message = xhr.responseJSON.message;

  return message || fallbackErrorMessage;
}

export default getErrorMessage;
