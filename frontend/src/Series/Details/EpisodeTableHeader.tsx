import React from 'react';
import Column from 'Components/Table/Column';
import TableOptionsButton from 'Components/Table/TableOptions/TableOptionsButton';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { TableOptionsChangePayload } from 'typings/Table';

interface EpisodeTableHeaderProps {
  columns: Column[];
  sortKey?: string;
  sortDirection?: SortDirection;
  onSortPress: (name: string, sortDirection?: SortDirection) => void;
  onTableOptionChange: (payload: TableOptionsChangePayload) => void;
}

function EpisodeTableHeader({
  columns,
  sortKey,
  sortDirection,
  onSortPress,
  onTableOptionChange,
}: EpisodeTableHeaderProps) {
  return (
    <VirtualTableHeader role="row">
      {columns.map((column) => {
        const { name, label, columnLabel, isSortable, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'actions') {
          return (
            <VirtualTableHeaderCell
              key={name}
              name={name}
              role="columnheader"
              isSortable={false}
            >
              <TableOptionsModalWrapper
                columns={columns}
                onTableOptionChange={onTableOptionChange}
              >
                <TableOptionsButton />
              </TableOptionsModalWrapper>
            </VirtualTableHeaderCell>
          );
        }

        return (
          <VirtualTableHeaderCell
            key={name}
            name={name}
            role="columnheader"
            title={
              typeof columnLabel === 'function' ? columnLabel() : columnLabel
            }
            isSortable={isSortable}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          >
            {typeof label === 'function' ? label() : label}
          </VirtualTableHeaderCell>
        );
      })}
    </VirtualTableHeader>
  );
}

export default EpisodeTableHeader;
