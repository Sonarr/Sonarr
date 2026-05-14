import AppSectionState, {
  AppSectionDeleteState,
  AppSectionItemState,
  AppSectionListState,
  AppSectionSaveState,
  AppSectionSchemaState,
} from 'App/State/AppSectionState';
import DelayProfile from 'typings/DelayProfile';
import DownloadClient from 'typings/DownloadClient';
import DownloadClientOptions from 'typings/Settings/DownloadClientOptions';

type Presets<T> = T & {
  presets: T[];
};

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

interface SettingsAppState {
  delayProfiles: DelayProfileAppState;
  downloadClients: DownloadClientAppState;
  downloadClientOptions: DownloadClientOptionsAppState;
}

export default SettingsAppState;
