import { createSelector } from 'reselect';
import { AppSectionItemState } from 'App/State/AppSectionState';
import AppState from 'App/State/AppState';
import SettingsAppState from 'App/State/SettingsAppState';
import selectSettings from 'Store/Selectors/selectSettings';
import { PendingSection } from 'typings/pending';

type SectionsWithItemNames = {
  [K in keyof SettingsAppState]: SettingsAppState[K] extends AppSectionItemState<unknown>
    ? K
    : never;
}[keyof SettingsAppState];

type GetSectionState<Name extends SectionsWithItemNames> =
  SettingsAppState[Name];
type GetSettingsSectionItemType<Name extends SectionsWithItemNames> =
  GetSectionState<Name> extends AppSectionItemState<infer R> ? R : never;

function createSettingsSectionSelector<
  Name extends SectionsWithItemNames,
  T extends GetSettingsSectionItemType<Name>
>(section: Name) {
  return createSelector(
    (state: AppState) => state.settings[section],
    (sectionSettings) => {
      const { item, pendingChanges, ...other } = sectionSettings;

      const saveError =
        'saveError' in sectionSettings ? sectionSettings.saveError : undefined;

      const {
        settings,
        pendingChanges: selectedPendingChanges,
        ...rest
      } = selectSettings(item, pendingChanges, saveError);

      return {
        ...other,
        saveError,
        settings: settings as PendingSection<T>,
        pendingChanges: selectedPendingChanges as Partial<T>,
        ...rest,
      };
    }
  );
}

export default createSettingsSectionSelector;
