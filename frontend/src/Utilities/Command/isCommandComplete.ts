import Command from 'Commands/Command';

function isCommandComplete(command: Command) {
  if (!command) {
    return false;
  }

  return command.status === 'completed';
}

export default isCommandComplete;
