import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';

export interface ManageDownloadClientsOptions {
  sortKey: string;
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
