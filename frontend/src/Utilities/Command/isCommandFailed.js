function isCommandFailed(command) {
  if (!command) {
    return false;
  }

  return command.state === 'failed' ||
         command.state === 'aborted' ||
         command.state === 'cancelled' ||
         command.state === 'orphaned';
}

export default isCommandFailed;
