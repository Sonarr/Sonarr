import { createSelector } from 'reselect';

function createCommandsSelector() {
  return createSelector(
    (state) => state.commands,
    (commands) => {
      return commands.items;
    }
  );
}

export default createCommandsSelector;
