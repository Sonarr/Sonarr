import { useManageSettings, useSettings } from 'Settings/useSettings';

export interface DownloadClientSettingsModel {
  downloadClientWorkingFolders: string;
  enableCompletedDownloadHandling: boolean;
  autoRedownloadFailed: boolean;
  autoRedownloadFailedFromInteractiveSearch: boolean;
}

const PATH = '/settings/downloadclient';

export const useDownloadClientSettings = () => {
  return useSettings<DownloadClientSettingsModel>(PATH);
};

export const useManageDownloadClientSettings = () => {
  return useManageSettings<DownloadClientSettingsModel>(PATH);
};
