import AppSectionState, {
  AppSectionDeleteState,
  AppSectionItemState,
  AppSectionSaveState,
  AppSectionSchemaState,
} from 'App/State/AppSectionState';
import Language from 'Language/Language';
import DownloadClient from 'typings/DownloadClient';
import ImportList from 'typings/ImportList';
import Indexer from 'typings/Indexer';
import Notification from 'typings/Notification';
import QualityProfile from 'typings/QualityProfile';
import { UiSettings } from 'typings/UiSettings';

export interface DownloadClientAppState
  extends AppSectionState<DownloadClient>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface ImportListAppState
  extends AppSectionState<ImportList>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface IndexerAppState
  extends AppSectionState<Indexer>,
    AppSectionDeleteState,
    AppSectionSaveState {}

export interface NotificationAppState
  extends AppSectionState<Notification>,
    AppSectionDeleteState {}

export interface QualityProfilesAppState
  extends AppSectionState<QualityProfile>,
    AppSectionSchemaState<QualityProfile> {}

export type LanguageSettingsAppState = AppSectionState<Language>;
export type UiSettingsAppState = AppSectionItemState<UiSettings>;

interface SettingsAppState {
  downloadClients: DownloadClientAppState;
  importLists: ImportListAppState;
  indexers: IndexerAppState;
  languages: LanguageSettingsAppState;
  notifications: NotificationAppState;
  qualityProfiles: QualityProfilesAppState;
  ui: UiSettingsAppState;
}

export default SettingsAppState;
