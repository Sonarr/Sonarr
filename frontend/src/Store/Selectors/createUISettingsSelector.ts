import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createUISettingsSelector() {
  return createSelector(
    (state: AppState) => state.settings.ui,
    (ui) => {
      return ui.item;
    }
  );
}

export default createUISettingsSelector;
