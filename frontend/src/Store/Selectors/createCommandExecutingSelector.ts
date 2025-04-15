import { createSelector } from 'reselect';
import { isCommandExecuting } from 'Utilities/Command';
import createCommandSelector from './createCommandSelector';

function createCommandExecutingSelector(name: string, constraints = {}) {
  return createSelector(createCommandSelector(name, constraints), (command) => {
    return command ? isCommandExecuting(command) : false;
  });
}

export default createCommandExecutingSelector;
