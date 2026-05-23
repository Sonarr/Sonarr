import AppSectionState, {
  AppSectionDeleteState,
  AppSectionSaveState,
  AppSectionSchemaState,
} from 'App/State/AppSectionState';
import DownloadClient from 'typings/DownloadClient';

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

interface SettingsAppState {
  downloadClients: DownloadClientAppState;
}

export default SettingsAppState;
