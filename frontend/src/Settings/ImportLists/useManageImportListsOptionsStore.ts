import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { ImportListModel } from 'Settings/ImportLists/ImportLists/useImportLists';

export interface ManageImportListsOptions {
  sortKey: keyof ImportListModel;
  sortDirection: SortDirection;
}

const { useOptions, setSort } = createOptionsStore<ManageImportListsOptions>(
  'manage_import_lists_options',
  () => {
    return {
      sortKey: 'name',
      sortDirection: 'ascending',
    };
  }
);

export const useManageImportListsOptions = useOptions;
export const setManageImportListsSort = setSort;
