import React from 'react';
import Column from 'Components/Table/Column';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import { SortDirection } from 'Helpers/Props/sortDirections';
import rowStyles from './InteractiveSearchRow.module.css';

const COLUMN_CLASS_NAMES: Record<string, string> = {
  protocol: 'protocol',
  age: 'age',
  title: 'title',
  indexer: 'indexer',
  history: 'history',
  size: 'size',
  peers: 'peers',
  languages: 'languages',
  qualityWeight: 'quality',
  customFormatScore: 'customFormatScore',
  indexerFlags: 'indexerFlags',
  rejections: 'rejected',
  releaseWeight: 'download',
};

interface InteractiveSearchTableHeaderProps {
  columns: Column[];
  sortKey?: string;
  sortDirection?: SortDirection;
  onSortPress: (sortKey: string, sortDirection?: SortDirection) => void;
}

function InteractiveSearchTableHeader({
  columns,
  sortKey,
  sortDirection,
  onSortPress,
}: InteractiveSearchTableHeaderProps) {
  return (
    <VirtualTableHeader>
      {columns.map((column) => {
        const { name, label, isSortable, isVisible, fixedSortDirection } =
          column;

        if (!isVisible) {
          return null;
        }

        return (
          <VirtualTableHeaderCell
            key={name}
            className={
              rowStyles[COLUMN_CLASS_NAMES[name] as keyof typeof rowStyles]
            }
            name={name}
            sortKey={sortKey}
            sortDirection={sortDirection}
            fixedSortDirection={fixedSortDirection}
            isSortable={isSortable}
            onSortPress={onSortPress}
          >
            {typeof label === 'function' ? label() : label}
          </VirtualTableHeaderCell>
        );
      })}
    </VirtualTableHeader>
  );
}

export default InteractiveSearchTableHeader;
