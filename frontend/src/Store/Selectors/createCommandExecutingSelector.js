import _ from 'lodash';
import { createSelector } from 'reselect';
import createCommandsSelector from './createCommandsSelector';

function createCommandExecutingSelector(name) {
  return createSelector(
    createCommandsSelector(),
    (commands) => {
      return _.some(commands, { name });
    }
  );
}

export default createCommandExecutingSelector;
