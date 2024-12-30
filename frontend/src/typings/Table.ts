import Column from 'Components/Table/Column';

export interface TableOptionsChangePayload {
  pageSize?: number;
  columns?: Column[];
}
