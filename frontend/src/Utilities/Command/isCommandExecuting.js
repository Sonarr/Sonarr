function isCommandExecuting(command) {
  if (!command) {
    return false;
  }

  return command.status === 'queued' || command.status === 'started';
}

export default isCommandExecuting;
