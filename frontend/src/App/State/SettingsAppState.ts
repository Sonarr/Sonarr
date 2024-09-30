import AppSectionState, {
  AppSectionDeleteState,
  AppSectionItemSchemaState,
  AppSectionItemState,
  AppSectionSaveState,
  PagedAppSectionState,
} from 'App/State/AppSectionState';
import Language from 'Language/Language';
import CustomFormat from 'typings/CustomFormat';
import DownloadClient from 'typings/DownloadClient';
import ImportList from 'typings/ImportList';
import ImportListExclusion from 'typings/ImportListExclusion';
import ImportListOptionsSettings from 'typings/ImportListOptionsSettings';
import Indexer from 'typings/Indexer';
import IndexerFlag from 'typings/IndexerFlag';
import Notification from 'typings/Notification';
import QualityProfile from 'typings/QualityProfile';
import General from 'typings/Settings/General';
import NamingConfig from 'typings/Settings/NamingConfig';
import NamingExample from 'typings/Settings/NamingExample';
import ReleaseProfile from 'typings/Settings/ReleaseProfile';
import UiSettings from 'typings/Settings/UiSettings';

export interface DownloadClientAppState
  extends AppSectionState<DownloadClient>,
    AppSectionDeleteState,
    AppSectionSaveState {
  isTestingAll: boolean;
}

export interface GeneralAppState
  extends AppSectionItemState<General>,
    AppSectionSaveState {}

export interface NamingAppState
  extends AppSectionItemState<NamingConfig>,
    AppSectionSaveState {}

export interface NamingExamplesAppState
  extends AppSectionItemState<NamingExample> {}

export interface ImportListAppState
  extends AppSectionState<ImportList>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface IndexerAppState
  extends AppSectionState<Indexer>,
    AppSectionDeleteState,
    AppSectionSaveState {
  isTestingAll: boolean;
}

export interface NotificationAppState
  extends AppSectionState<Notification>,
    AppSectionDeleteState {}

export interface QualityProfilesAppState
  extends AppSectionState<QualityProfile>,
    AppSectionItemSchemaState<QualityProfile> {}

export interface ReleaseProfilesAppState
  extends AppSectionState<ReleaseProfile>,
    AppSectionSaveState {
  pendingChanges: Partial<ReleaseProfile>;
}

export interface CustomFormatAppState
  extends AppSectionState<CustomFormat>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface ImportListOptionsSettingsAppState
  extends AppSectionItemState<ImportListOptionsSettings>,
    AppSectionSaveState {}

export interface ImportListExclusionsSettingsAppState
  extends AppSectionState<ImportListExclusion>,
    AppSectionSaveState,
    PagedAppSectionState,
    AppSectionDeleteState {
  pendingChanges: Partial<ImportListExclusion>;
}

export type IndexerFlagSettingsAppState = AppSectionState<IndexerFlag>;
export type LanguageSettingsAppState = AppSectionState<Language>;
export type UiSettingsAppState = AppSectionItemState<UiSettings>;

interface SettingsAppState {
  advancedSettings: boolean;
  customFormats: CustomFormatAppState;
  downloadClients: DownloadClientAppState;
  general: GeneralAppState;
  importListExclusions: ImportListExclusionsSettingsAppState;
  importListOptions: ImportListOptionsSettingsAppState;
  importLists: ImportListAppState;
  indexerFlags: IndexerFlagSettingsAppState;
  indexers: IndexerAppState;
  languages: LanguageSettingsAppState;
  naming: NamingAppState;
  namingExamples: NamingExamplesAppState;
  notifications: NotificationAppState;
  qualityProfiles: QualityProfilesAppState;
  releaseProfiles: ReleaseProfilesAppState;
  ui: UiSettingsAppState;
}

export default SettingsAppState;
