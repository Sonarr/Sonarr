import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createCommandsSelector() {
  return createSelector(
    (state: AppState) => state.commands,
    (commands) => {
      return commands.items;
    }
  );
}

export default createCommandsSelector;
