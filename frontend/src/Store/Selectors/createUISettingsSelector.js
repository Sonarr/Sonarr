import { createSelector } from 'reselect';

function createUISettingsSelector() {
  return createSelector(
    (state) => state.settings.ui,
    (ui) => {
      return ui.item;
    }
  );
}

export default createUISettingsSelector;
