import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';

function createSettingsSectionSelector() {
  return createSelector(
    (state, { section }) => state.settings[section],
    (sectionSettings) => {
      const {
        isFetching,
        isPopulated,
        error,
        item,
        pendingChanges,
        isSaving,
        saveError
      } = sectionSettings;

      const settings = selectSettings(item, pendingChanges, saveError);

      return {
        isFetching,
        isPopulated,
        error,
        isSaving,
        saveError,
        ...settings
      };
    }
  );
}

export default createSettingsSectionSelector;
