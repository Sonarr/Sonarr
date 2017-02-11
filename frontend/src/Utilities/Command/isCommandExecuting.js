function isCommandExecuting(command) {
  if (!command) {
    return false;
  }

  return command.state === 'queued' || command.state === 'started';
}

export default isCommandExecuting;
