import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { DownloadClientModel } from 'Settings/DownloadClients/DownloadClients/useDownloadClients';

export interface ManageDownloadClientsOptions {
  sortKey: keyof DownloadClientModel;
  sortDirection: SortDirection;
}

const { useOptions, setSort } =
  createOptionsStore<ManageDownloadClientsOptions>(
    'manage_download_clients_options',
    () => {
      return {
        sortKey: 'name',
        sortDirection: 'ascending',
      };
    }
  );

export const useManageDownloadClientsOptions = useOptions;
export const setManageDownloadClientsSort = setSort;
