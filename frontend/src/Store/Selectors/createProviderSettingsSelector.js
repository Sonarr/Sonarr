import _ from 'lodash';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';

function createProviderSettingsSelector() {
  return createSelector(
    (state, { id }) => id,
    (state, { section }) => state.settings[section],
    (id, section) => {
      if (!id) {
        const item = _.isArray(section.schema) ? section.selectedSchema : section.schema;
        const settings = selectSettings(Object.assign({ name: '' }, item), section.pendingChanges, section.saveError);

        const {
          isFetchingSchema: isFetching,
          schemaError: error,
          isSaving,
          saveError,
          isTesting,
          pendingChanges
        } = section;

        return {
          isFetching,
          error,
          isSaving,
          saveError,
          isTesting,
          pendingChanges,
          ...settings,
          item: settings.settings
        };
      }

      const {
        isFetching,
        error,
        isSaving,
        saveError,
        isTesting,
        pendingChanges
      } = section;

      const settings = selectSettings(_.find(section.items, { id }), pendingChanges, saveError);

      return {
        isFetching,
        error,
        isSaving,
        saveError,
        isTesting,
        item: settings.settings,
        ...settings
      };
    }
  );
}

export default createProviderSettingsSelector;
