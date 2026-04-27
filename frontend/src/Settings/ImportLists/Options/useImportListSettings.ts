import { useManageSettings, useSettings } from 'Settings/useSettings';

export type ListSyncLevel =
  | 'disabled'
  | 'logOnly'
  | 'keepAndUnmonitor'
  | 'keepAndTag';

export interface ImportListSettingsModel {
  listSyncLevel: ListSyncLevel;
  listSyncTag: number;
}

const PATH = '/settings/importlist';

export const useImportListSettings = () => {
  return useSettings<ImportListSettingsModel>(PATH);
};

export const useManageImportListSettings = () => {
  return useManageSettings<ImportListSettingsModel>(PATH);
};
