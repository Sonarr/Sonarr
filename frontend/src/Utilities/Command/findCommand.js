import _ from 'lodash';
import isSameCommand from './isSameCommand';

function findCommand(commands, options) {
  return _.findLast(commands, (command) => {
    return isSameCommand(command.body, options);
  });
}

export default findCommand;
