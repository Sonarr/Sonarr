import _ from 'lodash';
import { createSelector } from 'reselect';
import createCommandsSelector from './createCommandsSelector';

function createCommandSelector(name, contraints = {}) {
  return createSelector(
    createCommandsSelector(),
    (commands) => {
      return _.some(commands, { name, ...contraints });
    }
  );
}

export default createCommandSelector;
