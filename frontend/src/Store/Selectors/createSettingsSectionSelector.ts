import { createSelector } from 'reselect';
import AppSectionState, {
  AppSectionItemState,
} from 'App/State/AppSectionState';
import AppState from 'App/State/AppState';
import selectSettings from 'Store/Selectors/selectSettings';
import { PendingSection } from 'typings/pending';

type SettingNames = keyof Omit<AppState['settings'], 'advancedSettings'>;
type GetSectionState<Name extends SettingNames> = AppState['settings'][Name];
type GetSettingsSectionItemType<Name extends SettingNames> =
  GetSectionState<Name> extends AppSectionItemState<infer R>
    ? R
    : GetSectionState<Name> extends AppSectionState<infer R>
    ? R
    : never;

type AppStateWithPending<Name extends SettingNames> = {
  item?: GetSettingsSectionItemType<Name>;
  pendingChanges?: Partial<GetSettingsSectionItemType<Name>>;
  saveError?: Error;
} & GetSectionState<Name>;

function createSettingsSectionSelector<Name extends SettingNames>(
  section: Name
) {
  return createSelector(
    (state: AppState) => state.settings[section],
    (sectionSettings) => {
      const { item, pendingChanges, saveError, ...other } =
        sectionSettings as AppStateWithPending<Name>;

      const { settings, ...rest } = selectSettings(
        item,
        pendingChanges,
        saveError
      );

      return {
        ...other,
        saveError,
        settings: settings as PendingSection<GetSettingsSectionItemType<Name>>,
        ...rest,
      };
    }
  );
}

export default createSettingsSectionSelector;
