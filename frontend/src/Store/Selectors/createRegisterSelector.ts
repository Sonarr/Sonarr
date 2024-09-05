import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createRegisterSelector() {
  return createSelector(
    (state: AppState) => state.settings.register,
    (register) => {
      return register;
    }
  );
}

export default createRegisterSelector;
