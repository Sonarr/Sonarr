import AppSectionState, {
  AppSectionDeleteState,
  AppSectionItemState,
  AppSectionSaveState,
  AppSectionSchemaState,
} from 'App/State/AppSectionState';
import DownloadClient from 'typings/DownloadClient';
import DownloadClientOptions from 'typings/Settings/DownloadClientOptions';

type Presets<T> = T & {
  presets: T[];
};

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
  downloadClients: DownloadClientAppState;
  downloadClientOptions: DownloadClientOptionsAppState;
}

export default SettingsAppState;
