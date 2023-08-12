import React from 'react';

type PropertyFunction<T> = () => T;

interface Column {
  name: string;
  label: string | PropertyFunction<string> | React.ReactNode;
  columnLabel?: string;
  isSortable?: boolean;
  isVisible: boolean;
  isModifiable?: boolean;
}

export default Column;
