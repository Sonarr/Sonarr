import Column from 'Components/Table/Column';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { ValidationFailure } from 'typings/pending';
import { Filter, FilterBuilderProp } from './AppState';

export interface Error {
  status?: number;
  responseJSON:
    | {
        message: string | undefined;
      }
    | ValidationFailure[]
    | undefined;
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
  filters: Filter[];
  filterBuilderProps: FilterBuilderProp<T>[];
}

export interface AppSectionSchemaState<T> {
  isSchemaFetching: boolean;
  isSchemaPopulated: boolean;
  schemaError: Error;
  schema: T[];
  selectedSchema?: T;
}

export interface AppSectionItemSchemaState<T> {
  isSchemaFetching: boolean;
  isSchemaPopulated: boolean;
  schemaError: Error;
  schema: T;
}

export interface AppSectionItemState<T> {
  isFetching: boolean;
  isPopulated: boolean;
  error: Error;
  pendingChanges: Partial<T>;
  item: T;
}

export interface AppSectionProviderState<T>
  extends AppSectionDeleteState,
    AppSectionSaveState {
  isFetching: boolean;
  isPopulated: boolean;
  isTesting?: boolean;
  error: Error;
  items: T[];
  pendingChanges?: Partial<T>;
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
