import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createUserDetailsSelector() {
  return createSelector(
    (state: AppState) => state.settings.users.items,
    (users) => {
      return users;
    }
  );
}

export default createUserDetailsSelector;
