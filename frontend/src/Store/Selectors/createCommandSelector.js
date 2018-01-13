import { createSelector } from 'reselect';
import { findCommand } from 'Utilities/Command';
import createCommandsSelector from './createCommandsSelector';

function createCommandSelector(name, contraints = {}) {
  return createSelector(
    createCommandsSelector(),
    (commands) => {
      return findCommand(commands, { name, ...contraints });
    }
  );
}

export default createCommandSelector;
