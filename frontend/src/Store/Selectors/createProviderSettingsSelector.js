import _ from 'lodash';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';

function selector(id, section) {
  if (!id) {
    const item = _.isArray(section.schema) ? section.selectedSchema : section.schema;
    const settings = selectSettings(Object.assign({ name: '' }, item), section.pendingChanges, section.saveError);

    const {
      isSchemaFetching: isFetching,
      isSchemaPopulated: isPopulated,
      schemaError: error,
      isSaving,
      saveError,
      isTesting,
      pendingChanges
    } = section;

    return {
      isFetching,
      isPopulated,
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
    isPopulated,
    error,
    isSaving,
    saveError,
    isTesting,
    pendingChanges
  } = section;

  const settings = selectSettings(_.find(section.items, { id }), pendingChanges, saveError);

  return {
    isFetching,
    isPopulated,
    error,
    isSaving,
    saveError,
    isTesting,
    ...settings,
    item: settings.settings
  };
}

export default function createProviderSettingsSelector(sectionName) {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings[sectionName],
    (id, section) => selector(id, section)
  );
}

export function createProviderSettingsSelectorHook(sectionName, id) {
  return createSelector(
    (state) => state.settings[sectionName],
    (section) => selector(id, section)
  );
}

