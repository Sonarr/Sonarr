import { createSelector } from 'reselect';
import { findCommand } from 'Utilities/Command';
import createCommandsSelector from './createCommandsSelector';

function createCommandSelector(name: string, constraints = {}) {
  return createSelector(createCommandsSelector(), (commands) => {
    return findCommand(commands, { name, ...constraints });
  });
}

export default createCommandSelector;
