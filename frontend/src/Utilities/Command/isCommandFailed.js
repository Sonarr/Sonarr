function isCommandFailed(command) {
  if (!command) {
    return false;
  }

  return command.status === 'failed' ||
         command.status === 'aborted' ||
         command.status === 'cancelled' ||
         command.status === 'orphaned';
}

export default isCommandFailed;
