import _ from 'lodash';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';

function createSettingsSectionSelector() {
  return createSelector(
    (state, { section }) => _.get(state, section),
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
