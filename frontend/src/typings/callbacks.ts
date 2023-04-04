import SortDirection from 'Helpers/Props/SortDirection';

export type SortCallback = (
  sortKey: string,
  sortDirection: SortDirection
) => void;
