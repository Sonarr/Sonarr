import SortDirection from 'Helpers/Props/SortDirection';

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
  pageSize: number;
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
