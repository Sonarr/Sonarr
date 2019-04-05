import { createSelector } from 'reselect';
import { isCommandExecuting } from 'Utilities/Command';

function createExecutingCommandsSelector() {
  return createSelector(
    (state) => state.commands.items,
    (commands) => {
      return commands.filter((command) => isCommandExecuting(command));
    }
  );
}

export default createExecutingCommandsSelector;
