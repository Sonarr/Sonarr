import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';

export interface ImportListExclusionOptions {
  pageSize: number;
  sortKey: string;
  sortDirection: SortDirection;
}

const { useOptions, setOptions, setOption, setSort } =
  createOptionsStore<ImportListExclusionOptions>(
    'import_list_exclusion_options',
    () => {
      return {
        pageSize: 20,
        sortKey: 'id',
        sortDirection: 'descending',
      };
    }
  );

export const useImportListExclusionOptions = useOptions;
export const setImportListExclusionOptions = setOptions;
export const setImportListExclusionOption = setOption;
export const setImportListExclusionSort = setSort;
