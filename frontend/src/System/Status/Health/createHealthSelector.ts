import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createHealthSelector() {
  return createSelector(
    (state: AppState) => state.system.health,
    (health) => {
      return health;
    }
  );
}

export default createHealthSelector;
