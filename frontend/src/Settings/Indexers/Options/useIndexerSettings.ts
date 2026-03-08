import { useManageSettings, useSettings } from 'Settings/useSettings';

export interface IndexerSettingsModel {
  minimumAge: number;
  retention: number;
  maximumSize: number;
  rssSyncInterval: number;
}

const PATH = '/settings/indexer';

export const useIndexerSettings = () => {
  return useSettings<IndexerSettingsModel>(PATH);
};

export const useManageIndexerSettings = () => {
  return useManageSettings<IndexerSettingsModel>(PATH);
};
