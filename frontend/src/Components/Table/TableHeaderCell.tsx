import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons, sortDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import styles from './TableHeaderCell.css';

interface TableHeaderCellProps {
  className?: string;
  name: string;
  label?: string | (() => string) | React.ReactNode;
  columnLabel?: string | (() => string);
  isSortable?: boolean;
  isVisible?: boolean;
  isModifiable?: boolean;
  sortKey?: string;
  fixedSortDirection?: SortDirection;
  sortDirection?: string;
  children?: React.ReactNode;
  onSortPress?: (name: string, sortDirection?: SortDirection) => void;
}

function TableHeaderCell({
  className = styles.headerCell,
  name,
  label,
  columnLabel,
  isSortable = false,
  isVisible,
  isModifiable,
  sortKey,
  sortDirection,
  fixedSortDirection,
  children,
  onSortPress,
  ...otherProps
}: TableHeaderCellProps) {
  const isSorting = isSortable && sortKey === name;
  const sortIcon =
    sortDirection === sortDirections.ASCENDING
      ? icons.SORT_ASCENDING
      : icons.SORT_DESCENDING;

  const handlePress = useCallback(() => {
    if (fixedSortDirection) {
      onSortPress?.(name, fixedSortDirection);
    } else {
      onSortPress?.(name);
    }
  }, [name, fixedSortDirection, onSortPress]);

  return isSortable ? (
    <Link
      {...otherProps}
      component="th"
      className={className}
      // label={typeof label === 'function' ? label() : label}
      title={typeof columnLabel === 'function' ? columnLabel() : columnLabel}
      onPress={handlePress}
    >
      {children}

      {isSorting && <Icon name={sortIcon} className={styles.sortIcon} />}
    </Link>
  ) : (
    <th className={className}>{children}</th>
  );
}

export default TableHeaderCell;
