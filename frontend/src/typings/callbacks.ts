import { SortDirection } from 'Helpers/Props/sortDirections';

export type SortCallback = (
  sortKey: string,
  sortDirection: SortDirection
) => void;
