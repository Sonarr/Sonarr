import React from 'react';
import { SortDirection } from 'Helpers/Props/sortDirections';

type PropertyFunction<T> = () => T;
export type IsModifiable = 'disabled' | 'enabled' | 'onlyPosition';

// TODO: Convert to generic so `name` can be a type
interface Column {
  name: string;
  label: string | PropertyFunction<string> | React.ReactNode;
  className?: string;
  columnLabel?: string | PropertyFunction<string>;
  isSortable?: boolean;
  fixedSortDirection?: SortDirection;
  isVisible: boolean;
  isModifiable?: IsModifiable;
}

export default Column;
