import { Error } from 'App/State/AppSectionState';
import { ApiError } from 'Helpers/Hooks/useApiQuery';

function getErrorMessage(
  error: Error | ApiError | undefined,
  fallbackErrorMessage = ''
) {
  if (!error) {
    return fallbackErrorMessage;
  }

  if (error instanceof ApiError) {
    if (!error.statusBody) {
      return fallbackErrorMessage;
    }

    return error.statusBody.message;
  }

  if (!error.responseJSON) {
    return fallbackErrorMessage;
  }

  if ('message' in error.responseJSON && error.responseJSON.message) {
    return error.responseJSON.message;
  }

  return fallbackErrorMessage;
}

export default getErrorMessage;
