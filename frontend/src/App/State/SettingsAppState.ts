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
import CustomFormatSpecification from 'typings/CustomFormatSpecification';
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
import DownloadClientOptions from 'typings/Settings/DownloadClientOptions';
import General from 'typings/Settings/General';
import IndexerOptions from 'typings/Settings/IndexerOptions';
import MediaManagement from 'typings/Settings/MediaManagement';
import NamingConfig from 'typings/Settings/NamingConfig';
import NamingExample from 'typings/Settings/NamingExample';
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
    AppSectionSaveState,
    AppSectionSchemaState<Presets<DownloadClient>> {
  isTestingAll: boolean;
}

export interface DownloadClientOptionsAppState
  extends AppSectionItemState<DownloadClientOptions>,
    AppSectionSaveState {}

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
    AppSectionSaveState,
    AppSectionSchemaState<Presets<ImportList>> {
  isTestingAll: boolean;
}

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
    AppSectionDeleteState,
    AppSectionSaveState,
    AppSectionSchemaState<Presets<Notification>> {}

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

export interface CustomFormatAppState
  extends AppSectionState<CustomFormat>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface CustomFormatSpecificationAppState
  extends AppSectionState<CustomFormatSpecification>,
    AppSectionDeleteState,
    AppSectionSaveState,
    AppSectionSchemaState<Presets<CustomFormatSpecification>> {}

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

interface SettingsAppState {
  autoTaggings: AutoTaggingAppState;
  autoTaggingSpecifications: AutoTaggingSpecificationAppState;
  customFormats: CustomFormatAppState;
  customFormatSpecifications: CustomFormatSpecificationAppState;
  delayProfiles: DelayProfileAppState;
  downloadClients: DownloadClientAppState;
  downloadClientOptions: DownloadClientOptionsAppState;
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
}

export default SettingsAppState;
