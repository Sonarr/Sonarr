import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { isCommandExecuting } from 'Utilities/Command';

function createExecutingCommandsSelector() {
  return createSelector(
    (state: AppState) => state.commands.items,
    (commands) => {
      return commands.filter((command) => isCommandExecuting(command));
    }
  );
}

export default createExecutingCommandsSelector;
