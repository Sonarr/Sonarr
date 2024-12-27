import _ from 'lodash';
import { createSelector } from 'reselect';
import ModelBase from 'App/ModelBase';
import {
  AppSectionProviderState,
  AppSectionSchemaState,
} from 'App/State/AppSectionState';
import AppState from 'App/State/AppState';
import selectSettings, {
  ModelBaseSetting,
} from 'Store/Selectors/selectSettings';
import getSectionState from 'Utilities/State/getSectionState';

function selector<
  T extends ModelBaseSetting,
  S extends AppSectionProviderState<T> & AppSectionSchemaState<T>
>(id: number | undefined, section: S) {
  if (!id) {
    const item = _.isArray(section.schema)
      ? section.selectedSchema
      : section.schema;
    const settings = selectSettings(
      Object.assign({ name: '' }, item),
      section.pendingChanges ?? {},
      section.saveError
    );

    const {
      isSchemaFetching: isFetching,
      isSchemaPopulated: isPopulated,
      schemaError: error,
      isSaving,
      saveError,
      isTesting,
      pendingChanges,
    } = section;

    return {
      isFetching,
      isPopulated,
      error,
      isSaving,
      saveError,
      isTesting,
      ...settings,
      pendingChanges,
      item: settings.settings,
    };
  }

  const {
    isFetching,
    isPopulated,
    error,
    isSaving,
    saveError,
    isTesting,
    pendingChanges,
  } = section;

  const item = section.items.find((i) => i.id === id)!;
  const settings = selectSettings<T>(item, pendingChanges, saveError);

  return {
    isFetching,
    isPopulated,
    error,
    isSaving,
    saveError,
    isTesting,
    ...settings,
    item: settings.settings,
  };
}

export default function createProviderSettingsSelector<
  T extends ModelBase,
  S extends AppSectionProviderState<T> & AppSectionSchemaState<T>
>(sectionName: string) {
  // @ts-expect-error - This isn't fully typed
  return createSelector(
    (_state: AppState, { id }: { id: number }) => id,
    (state) => state.settings[sectionName] as S,
    (id: number, section: S) => selector(id, section)
  );
}

export function createProviderSettingsSelectorHook<
  T extends ModelBaseSetting,
  S extends AppSectionProviderState<T> & AppSectionSchemaState<T>
>(sectionName: string, id: number | undefined) {
  return createSelector(
    (state: AppState) => state.settings,
    (state) => {
      const sectionState = getSectionState(state, sectionName, false) as S;

      return selector<T, S>(id, sectionState);
    }
  );
}
