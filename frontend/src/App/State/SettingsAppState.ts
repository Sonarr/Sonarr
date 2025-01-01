import AppSectionState, {
  AppSectionDeleteState,
  AppSectionItemSchemaState,
  AppSectionItemState,
  AppSectionListState,
  AppSectionSaveState,
  AppSectionSchemaState,
  PagedAppSectionState,
} from 'App/State/AppSectionState';
import Language from 'Language/Language';
import AutoTagging, { AutoTaggingSpecification } from 'typings/AutoTagging';
import CustomFormat from 'typings/CustomFormat';
import DelayProfile from 'typings/DelayProfile';
import DownloadClient from 'typings/DownloadClient';
import ImportList from 'typings/ImportList';
import ImportListExclusion from 'typings/ImportListExclusion';
import ImportListOptionsSettings from 'typings/ImportListOptionsSettings';
import Indexer from 'typings/Indexer';
import IndexerFlag from 'typings/IndexerFlag';
import Notification from 'typings/Notification';
import QualityDefinition from 'typings/QualityDefinition';
import QualityProfile from 'typings/QualityProfile';
import General from 'typings/Settings/General';
import IndexerOptions from 'typings/Settings/IndexerOptions';
import MediaManagement from 'typings/Settings/MediaManagement';
import NamingConfig from 'typings/Settings/NamingConfig';
import NamingExample from 'typings/Settings/NamingExample';
import ReleaseProfile from 'typings/Settings/ReleaseProfile';
import UiSettings from 'typings/Settings/UiSettings';
import MetadataAppState from './MetadataAppState';

type Presets<T> = T & {
  presets: T[];
};

export interface AutoTaggingAppState
  extends AppSectionState<AutoTagging>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface AutoTaggingSpecificationAppState
  extends AppSectionState<AutoTaggingSpecification>,
    AppSectionDeleteState,
    AppSectionSaveState,
    AppSectionSchemaState<AutoTaggingSpecification> {}

export interface DelayProfileAppState
  extends AppSectionListState<DelayProfile>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface DownloadClientAppState
  extends AppSectionState<DownloadClient>,
    AppSectionDeleteState,
    AppSectionSaveState {
  isTestingAll: boolean;
}

export interface GeneralAppState
  extends AppSectionItemState<General>,
    AppSectionSaveState {}

export interface MediaManagementAppState
  extends AppSectionItemState<MediaManagement>,
    AppSectionSaveState {}

export interface NamingAppState
  extends AppSectionItemState<NamingConfig>,
    AppSectionSaveState {}

export type NamingExamplesAppState = AppSectionItemState<NamingExample>;

export interface ImportListAppState
  extends AppSectionState<ImportList>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface IndexerOptionsAppState
  extends AppSectionItemState<IndexerOptions>,
    AppSectionSaveState {}

export interface IndexerAppState
  extends AppSectionState<Indexer>,
    AppSectionDeleteState,
    AppSectionSaveState,
    AppSectionSchemaState<Presets<Indexer>> {
  isTestingAll: boolean;
}

export interface NotificationAppState
  extends AppSectionState<Notification>,
    AppSectionDeleteState {}

export interface QualityDefinitionsAppState
  extends AppSectionState<QualityDefinition>,
    AppSectionSaveState {
  pendingChanges: {
    [key: number]: Partial<QualityProfile>;
  };
}

export interface QualityProfilesAppState
  extends AppSectionState<QualityProfile>,
    AppSectionItemSchemaState<QualityProfile>,
    AppSectionDeleteState,
    AppSectionSaveState {}

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
  autoTaggings: AutoTaggingAppState;
  autoTaggingSpecifications: AutoTaggingSpecificationAppState;
  customFormats: CustomFormatAppState;
  delayProfiles: DelayProfileAppState;
  downloadClients: DownloadClientAppState;
  general: GeneralAppState;
  importListExclusions: ImportListExclusionsSettingsAppState;
  importListOptions: ImportListOptionsSettingsAppState;
  importLists: ImportListAppState;
  indexerFlags: IndexerFlagSettingsAppState;
  indexerOptions: IndexerOptionsAppState;
  indexers: IndexerAppState;
  languages: LanguageSettingsAppState;
  mediaManagement: MediaManagementAppState;
  metadata: MetadataAppState;
  naming: NamingAppState;
  namingExamples: NamingExamplesAppState;
  notifications: NotificationAppState;
  qualityDefinitions: QualityDefinitionsAppState;
  qualityProfiles: QualityProfilesAppState;
  releaseProfiles: ReleaseProfilesAppState;
  ui: UiSettingsAppState;
}

export default SettingsAppState;
