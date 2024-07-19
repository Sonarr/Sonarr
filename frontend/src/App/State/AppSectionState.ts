import Column from 'Components/Table/Column';
import SortDirection from 'Helpers/Props/SortDirection';
import { FilterBuilderProp, PropertyFilter } from './AppState';

export interface Error {
  responseJSON: {
    message: string;
  };
}

export interface AppSectionDeleteState {
  isDeleting: boolean;
  deleteError: Error;
}

export interface AppSectionSaveState {
  isSaving: boolean;
  saveError: Error;
}

export interface PagedAppSectionState {
  page: number;
  pageSize: number;
  totalPages: number;
  totalRecords?: number;
}
export interface TableAppSectionState {
  columns: Column[];
}

export interface AppSectionFilterState<T> {
  selectedFilterKey: string;
  filters: PropertyFilter[];
  filterBuilderProps: FilterBuilderProp<T>[];
}

export interface AppSectionSchemaState<T> {
  isSchemaFetching: boolean;
  isSchemaPopulated: boolean;
  schemaError: Error;
  schema: {
    items: T[];
  };
}

export interface AppSectionItemState<T> {
  isFetching: boolean;
  isPopulated: boolean;
  error: Error;
  pendingChanges: Partial<T>;
  item: T;
}

interface AppSectionState<T> {
  isFetching: boolean;
  isPopulated: boolean;
  error: Error;
  items: T[];
  sortKey: string;
  sortDirection: SortDirection;
}

export default AppSectionState;
