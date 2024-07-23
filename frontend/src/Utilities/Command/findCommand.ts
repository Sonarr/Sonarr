import _ from 'lodash';
import Command, { CommandBody } from 'Commands/Command';
import isSameCommand from './isSameCommand';

function findCommand(commands: Command[], options: Partial<CommandBody>) {
  return _.findLast(commands, (command) => {
    return isSameCommand(command.body, options);
  });
}

export default findCommand;
