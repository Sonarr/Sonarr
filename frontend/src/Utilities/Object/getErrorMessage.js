function getErrorMessage(xhr, fallbackErrorMessage) {
  if (!xhr || !xhr.responseJSON || !xhr.responseJSON.message) {
    return fallbackErrorMessage;
  }

  const message = xhr.responseJSON.message;

  return message || fallbackErrorMessage;
}

export default getErrorMessage;
