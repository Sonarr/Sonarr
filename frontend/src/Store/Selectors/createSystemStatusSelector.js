import { createSelector } from 'reselect';

function createSystemStatusSelector() {
  return createSelector(
    (state) => state.system.status,
    (status) => {
      return status.item;
    }
  );
}

export default createSystemStatusSelector;
