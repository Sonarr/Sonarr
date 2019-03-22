function isCommandComplete(command) {
  if (!command) {
    return false;
  }

  return command.status === 'complete';
}

export default isCommandComplete;
