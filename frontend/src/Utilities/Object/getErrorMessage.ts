import { Error } from 'App/State/AppSectionState';

function getErrorMessage(xhr: Error, fallbackErrorMessage?: string) {
  if (!xhr || !xhr.responseJSON) {
    return fallbackErrorMessage;
  }

  if ('message' in xhr.responseJSON && xhr.responseJSON.message) {
    return xhr.responseJSON.message;
  }

  return fallbackErrorMessage;
}

export default getErrorMessage;
