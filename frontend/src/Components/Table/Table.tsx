import classNames from 'classnames';
import React from 'react';
import IconButton from 'Components/Link/IconButton';
import Scroller from 'Components/Scroller/Scroller';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import { icons, scrollDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { CheckInputChanged } from 'typings/inputs';
import { TableOptionsChangePayload } from 'typings/Table';
import Column from './Column';
import TableHeader from './TableHeader';
import TableHeaderCell from './TableHeaderCell';
import TableSelectAllHeaderCell from './TableSelectAllHeaderCell';
import styles from './Table.css';

interface TableProps {
  className?: string;
  horizontalScroll?: boolean;
  selectAll?: boolean;
  allSelected?: boolean;
  allUnselected?: boolean;
  columns: Column[];
  optionsComponent?: React.ElementType;
  pageSize?: number;
  canModifyColumns?: boolean;
  sortKey?: string;
  sortDirection?: SortDirection;
  children?: React.ReactNode;
  onSortPress?: (name: string, sortDirection?: SortDirection) => void;
  onTableOptionChange?: (payload: TableOptionsChangePayload) => void;
  onSelectAllChange?: (change: CheckInputChanged) => void;
}

function Table({
  className = styles.table,
  horizontalScroll = true,
  selectAll = false,
  allSelected = false,
  allUnselected = false,
  columns,
  optionsComponent,
  pageSize,
  canModifyColumns,
  sortKey,
  sortDirection,
  children,
  onSortPress,
  onTableOptionChange,
  onSelectAllChange,
}: TableProps) {
  return (
    <Scroller
      className={classNames(
        styles.tableContainer,
        horizontalScroll && styles.horizontalScroll
      )}
      scrollDirection={
        horizontalScroll ? scrollDirections.HORIZONTAL : scrollDirections.NONE
      }
      autoFocus={false}
    >
      <table className={className}>
        <TableHeader>
          {selectAll && onSelectAllChange ? (
            <TableSelectAllHeaderCell
              allSelected={allSelected}
              allUnselected={allUnselected}
              onSelectAllChange={onSelectAllChange}
            />
          ) : null}

          {columns.map((column) => {
            const { name, isVisible, isSortable, ...otherColumnProps } = column;

            if (!isVisible) {
              return null;
            }

            if (
              (name === 'actions' || name === 'details') &&
              onTableOptionChange
            ) {
              return (
                <TableHeaderCell
                  key={name}
                  name={name}
                  isSortable={false}
                  {...otherColumnProps}
                >
                  <TableOptionsModalWrapper
                    columns={columns}
                    optionsComponent={optionsComponent}
                    pageSize={pageSize}
                    canModifyColumns={canModifyColumns}
                    onTableOptionChange={onTableOptionChange}
                  >
                    <IconButton name={icons.ADVANCED_SETTINGS} />
                  </TableOptionsModalWrapper>
                </TableHeaderCell>
              );
            }

            return (
              <TableHeaderCell
                key={column.name}
                {...column}
                sortKey={sortKey}
                sortDirection={sortDirection}
                onSortPress={onSortPress}
              >
                {typeof column.label === 'function'
                  ? column.label()
                  : column.label}
              </TableHeaderCell>
            );
          })}
        </TableHeader>
        {children}
      </table>
    </Scroller>
  );
}

export default Table;
