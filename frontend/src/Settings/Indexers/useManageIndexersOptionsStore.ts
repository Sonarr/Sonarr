import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';

export interface ManageIndexersOptions {
  sortKey: string;
  sortDirection: SortDirection;
}

const { useOptions, setSort } = createOptionsStore<ManageIndexersOptions>(
  'manage_indexers_options',
  () => {
    return {
      sortKey: 'name',
      sortDirection: 'ascending',
    };
  }
);

export const useManageIndexersOptions = useOptions;
export const setManageIndexersSort = setSort;
