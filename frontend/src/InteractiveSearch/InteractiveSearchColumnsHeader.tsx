import React from 'react';
import Icon from 'Components/Icon';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import { icons } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import translate from 'Utilities/String/translate';
import styles from './InteractiveSearchColumnsHeader.module.css';

const COLUMNS = [
  {
    name: 'title',
    className: styles.release,
    label: () => translate('Release'),
  },
  { name: 'age', className: styles.age, label: () => translate('Age') },
  { name: 'size', className: styles.size, label: () => translate('Size') },
  { name: 'peers', className: styles.peers, label: () => translate('Peers') },
  {
    name: 'qualityWeight',
    className: styles.quality,
    label: () => translate('Quality'),
  },
  {
    name: 'customFormatScore',
    className: styles.score,
    label: () => translate('Score'),
  },
];

interface InteractiveSearchColumnsHeaderProps {
  sortKey?: string;
  sortDirection?: SortDirection;
  onSortPress: (sortKey: string, sortDirection?: SortDirection) => void;
}

function InteractiveSearchColumnsHeader({
  sortKey,
  sortDirection,
  onSortPress,
}: InteractiveSearchColumnsHeaderProps) {
  return (
    <div className={styles.header}>
      <VirtualTableHeader>
        {COLUMNS.map(({ name, className, label }) => (
          <VirtualTableHeaderCell
            key={name}
            className={className}
            name={name}
            isSortable={true}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          >
            {label()}
          </VirtualTableHeaderCell>
        ))}

        <VirtualTableHeaderCell
          className={styles.actions}
          name="releaseWeight"
          isSortable={true}
          sortKey={sortKey}
          sortDirection={sortDirection}
          onSortPress={onSortPress}
        >
          <Icon name={icons.DOWNLOAD} title={translate('Preferred')} />
        </VirtualTableHeaderCell>
      </VirtualTableHeader>
    </div>
  );
}

export default InteractiveSearchColumnsHeader;
