import classNames from 'classnames';
import React, { useCallback, useMemo } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons, sortDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import styles from './VirtualTableHeaderCell.module.css';

interface VirtualTableHeaderCellProps {
  className?: string;
  name: string;
  role?: string;
  title?: string;
  isSortable?: boolean;
  sortKey?: string;
  fixedSortDirection?: SortDirection;
  sortDirection?: string;
  children?: React.ReactNode;
  onSortPress?: (name: string, sortDirection?: SortDirection) => void;
}

function VirtualTableHeaderCell({
  className = styles.headerCell,
  name,
  role,
  isSortable = false,
  sortKey,
  sortDirection,
  fixedSortDirection,
  children,
  onSortPress,
  ...otherProps
}: VirtualTableHeaderCellProps) {
  const isSorting = isSortable && sortKey === name;
  const sortIcon =
    sortDirection === sortDirections.ASCENDING
      ? icons.SORT_ASCENDING
      : icons.SORT_DESCENDING;

  const ariaSortValue = useMemo(() => {
    if (!role || !isSortable) {
      return undefined;
    }

    if (!isSorting) {
      return 'none';
    }

    return sortDirection === sortDirections.ASCENDING
      ? 'ascending'
      : 'descending';
  }, [role, isSorting, sortDirection, isSortable]);

  const handlePress = useCallback(() => {
    if (fixedSortDirection) {
      onSortPress?.(name, fixedSortDirection);
    } else {
      onSortPress?.(name);
    }
  }, [name, fixedSortDirection, onSortPress]);

  return isSortable ? (
    <Link
      component="div"
      className={classNames(
        className,
        styles.sortable,
        isSorting && styles.sorted
      )}
      role={role}
      aria-sort={ariaSortValue}
      onPress={handlePress}
      {...otherProps}
    >
      <span className={styles.content}>
        {children}

        {isSorting ? (
          <Icon
            name={sortIcon}
            className={styles.sortIcon}
            size={12}
            aria-hidden={true}
          />
        ) : null}
      </span>
    </Link>
  ) : (
    <div className={className} role={role}>
      {children}
    </div>
  );
}

export default VirtualTableHeaderCell;
