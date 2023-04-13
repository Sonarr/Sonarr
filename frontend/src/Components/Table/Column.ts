import React from 'react';

interface Column {
  name: string;
  label: string | React.ReactNode;
  columnLabel?: string;
  isSortable?: boolean;
  isVisible: boolean;
  isModifiable?: boolean;
}

export default Column;
