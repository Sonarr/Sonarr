import { createSelector } from 'reselect';
import { isCommandExecuting } from 'Utilities/Command';
import createCommandSelector from './createCommandSelector';

function createCommandExecutingSelector(name, contraints = {}) {
  return createSelector(
    createCommandSelector(name, contraints),
    (command) => {
      return isCommandExecuting(command);
    }
  );
}

export default createCommandExecutingSelector;
