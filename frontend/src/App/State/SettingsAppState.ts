import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSchemaState,
} from 'App/State/AppSectionState';
import Language from 'Language/Language';
import DownloadClient from 'typings/DownloadClient';
import QualityProfile from 'typings/QualityProfile';
import { UiSettings } from 'typings/UiSettings';

export interface DownloadClientAppState
  extends AppSectionState<DownloadClient>,
    AppSectionDeleteState {}

export interface QualityProfilesAppState
  extends AppSectionState<QualityProfile>,
    AppSectionSchemaState<QualityProfile> {}

export type LanguageSettingsAppState = AppSectionState<Language>;
export type UiSettingsAppState = AppSectionState<UiSettings>;

interface SettingsAppState {
  downloadClients: DownloadClientAppState;
  language: LanguageSettingsAppState;
  uiSettings: UiSettingsAppState;
  qualityProfiles: QualityProfilesAppState;
}

export default SettingsAppState;
