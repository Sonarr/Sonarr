import React, { RefObject, useRef } from 'react';
import { FixedSizeList, ListChildComponentProps } from 'react-window';
import { useAppDimension } from 'App/appStore';
import { useSelect } from 'App/Select/SelectContext';
import VirtualTable from 'Components/Table/VirtualTable';
import ImportSeriesRow from './ImportSeriesRow';
import {
  UnamppedFolderItem,
  useEnsureImportSeriesItems,
} from './importSeriesStore';
import styles from './ImportSeriesTable.css';

const ROW_HEIGHT = 158;
const ROW_HEIGHT_SMALL_SCREEN = 228;

interface RowItemData {
  items: UnamppedFolderItem[];
}

interface ImportSeriesTableProps {
  items: UnamppedFolderItem[];
  scrollerRef: RefObject<HTMLElement>;
}

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items } = data;

  if (index >= items.length) {
    return null;
  }

  const item = items[index];

  return (
    <div style={style} className={styles.row}>
      <ImportSeriesRow key={item.id} unmappedFolder={item} />
    </div>
  );
}

function ImportSeriesTable({ items, scrollerRef }: ImportSeriesTableProps) {
  const isSmallScreen = useAppDimension('isSmallScreen');
  const { useHasItems } = useSelect();

  const listRef = useRef<FixedSizeList<RowItemData>>(null);

  const hasSelectItems = useHasItems();

  useEnsureImportSeriesItems(items);

  if (!items.length || !hasSelectItems) {
    return null;
  }

  return (
    <VirtualTable
      itemCount={items.length}
      itemData={{
        items,
      }}
      isSmallScreen={isSmallScreen}
      listRef={listRef}
      rowHeight={isSmallScreen ? ROW_HEIGHT_SMALL_SCREEN : ROW_HEIGHT}
      Row={Row}
      scrollerRef={scrollerRef}
    />
  );
}

export default ImportSeriesTable;
