import { useCallback } from 'react';
import {
  useManageSettings,
  useSaveSettings,
  useSettings,
} from 'Settings/useSettings';

export interface MediaManagementSettingsModel {
  createEmptySeriesFolders: boolean;
  deleteEmptyFolders: boolean;
  episodeTitleRequired: string;
  skipFreeSpaceCheckWhenImporting: boolean;
  minimumFreeSpaceWhenImporting: number;
  copyUsingHardlinks: boolean;
  useScriptImport: boolean;
  scriptImportPath: string;
  importExtraFiles: boolean;
  extraFileExtensions: string;
  userRejectedExtensions: string;
  autoUnmonitorPreviouslyDownloadedEpisodes: boolean;
  downloadPropersAndRepacks: string;
  enableMediaInfo: boolean;
  rescanAfterRefresh: string;
  setPermissionsLinux: boolean;
  chmodFolder: string;
  chownGroup: string;
  fileDate: string;
  recycleBin: string;
  recycleBinCleanupDays: number;
  allowFingerprinting: string;
  seasonPackUpgrade: string;
  seasonPackUpgradeThreshold: number;
}

const PATH = '/settings/mediamanagement';

export const useMediaManagementSettingsValues = () => {
  const { data } = useSettings<MediaManagementSettingsModel>(PATH);

  return data;
};

export const useMediaManagementSettings = () => {
  return useSettings<MediaManagementSettingsModel>(PATH);
};

export const useManageMediaManagementSettings = () => {
  return useManageSettings<MediaManagementSettingsModel>(PATH);
};

export const useSaveMediaManagementSettings = () => {
  const { data } = useSettings<MediaManagementSettingsModel>(PATH);
  const { save } = useSaveSettings<MediaManagementSettingsModel>(PATH);

  const saveSettings = useCallback(
    (changes: Partial<MediaManagementSettingsModel>) => {
      const updatedSettings = { ...data, ...changes };

      save(updatedSettings);
    },
    [data, save]
  );

  return saveSettings;
};
