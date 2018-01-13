function isCommandComplete(command) {
  if (!command) {
    return false;
  }

  return command.state === 'complete';
}

export default isCommandComplete;
