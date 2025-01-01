import { createSelector } from 'reselect';
import ModelBase from 'App/ModelBase';
import {
  AppSectionItemSchemaState,
  AppSectionProviderState,
  AppSectionSchemaState,
} from 'App/State/AppSectionState';
import AppState from 'App/State/AppState';
import selectSettings, {
  ModelBaseSetting,
} from 'Store/Selectors/selectSettings';
import { PendingSection } from 'typings/pending';
import getSectionState from 'Utilities/State/getSectionState';

type SchemaState<T> = AppSectionSchemaState<T> | AppSectionItemSchemaState<T>;

function selector<
  T extends ModelBaseSetting,
  S extends AppSectionProviderState<T> & SchemaState<T>
>(id: number | undefined, section: S) {
  if (id) {
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

  const item =
    'selectedSchema' in section
      ? section.selectedSchema
      : (section.schema as T);

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
    item: settings.settings as PendingSection<T>,
  };
}

export default function createProviderSettingsSelector<
  T extends ModelBase,
  S extends AppSectionProviderState<T> & SchemaState<T>
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
  S extends AppSectionProviderState<T> & SchemaState<T>
>(sectionName: string, id: number | undefined) {
  return createSelector(
    (state: AppState) => state.settings,
    (state) => {
      const sectionState = getSectionState(state, sectionName, false) as S;

      return selector<T, S>(id, sectionState);
    }
  );
}
