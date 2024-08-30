import React from 'react';
import { SortDirection } from 'Helpers/Props/sortDirections';

type PropertyFunction<T> = () => T;

// TODO: Convert to generic so `name` can be a type
interface Column {
  name: string;
  label: string | PropertyFunction<string> | React.ReactNode;
  className?: string;
  columnLabel?: string;
  isSortable?: boolean;
  fixedSortDirection?: SortDirection;
  isVisible: boolean;
  isModifiable?: boolean;
}

export default Column;
