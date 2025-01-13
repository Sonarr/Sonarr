import React, { RefObject, useEffect, useMemo, useRef } from 'react';
import { useSelector } from 'react-redux';
import { FixedSizeList, ListChildComponentProps } from 'react-window';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Column from 'Components/Table/Column';
import VirtualTable from 'Components/Table/VirtualTable';
import { SortDirection } from 'Helpers/Props/sortDirections';
import Series from 'Series/Series';
import getIndexOfFirstCharacter from 'Utilities/Array/getIndexOfFirstCharacter';
import selectTableOptions from './selectTableOptions';
import SeriesIndexRow from './SeriesIndexRow';
import SeriesIndexTableHeader from './SeriesIndexTableHeader';
import styles from './SeriesIndexTable.css';

interface RowItemData {
  items: Series[];
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

interface SeriesIndexTableProps {
  items: Series[];
  sortKey: string;
  sortDirection?: SortDirection;
  jumpToCharacter?: string;
  scrollTop?: number;
  scrollerRef: RefObject<HTMLElement>;
  isSelectMode: boolean;
  isSmallScreen: boolean;
}

const columnsSelector = createSelector(
  (state: AppState) => state.seriesIndex.columns,
  (columns) => columns
);

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items, sortKey, columns, isSelectMode } = data;

  if (index >= items.length) {
    return null;
  }

  const series = items[index];

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        ...style,
      }}
      className={styles.row}
    >
      <SeriesIndexRow
        seriesId={series.id}
        sortKey={sortKey}
        columns={columns}
        isSelectMode={isSelectMode}
      />
    </div>
  );
}

function SeriesIndexTable(props: SeriesIndexTableProps) {
  const {
    items,
    sortKey,
    sortDirection,
    jumpToCharacter,
    isSelectMode,
    isSmallScreen,
    scrollerRef,
  } = props;

  const columns = useSelector(columnsSelector);
  const { showBanners } = useSelector(selectTableOptions);
  const listRef = useRef<FixedSizeList<RowItemData>>(null);

  const rowHeight = useMemo(() => {
    return showBanners ? 70 : 38;
  }, [showBanners]);

  useEffect(() => {
    if (jumpToCharacter) {
      const index = getIndexOfFirstCharacter(items, jumpToCharacter);

      if (index != null) {
        let scrollTop = index * rowHeight;

        // If the offset is zero go to the top, otherwise offset
        // by the approximate size of the header + padding (37 + 20).
        if (scrollTop > 0) {
          const offset = 57;

          scrollTop += offset;
        }

        listRef.current?.scrollTo(scrollTop);
        scrollerRef?.current?.scrollTo(0, scrollTop);
      }
    }
  }, [jumpToCharacter, rowHeight, items, scrollerRef, listRef]);

  return (
    <VirtualTable
      Header={
        <SeriesIndexTableHeader
          showBanners={showBanners}
          columns={columns}
          sortKey={sortKey}
          sortDirection={sortDirection}
          isSelectMode={isSelectMode}
        />
      }
      itemCount={items.length}
      itemData={{
        items,
        sortKey,
        columns,
        isSelectMode,
      }}
      isSmallScreen={isSmallScreen}
      listRef={listRef}
      rowHeight={rowHeight}
      Row={Row}
      scrollerRef={scrollerRef}
    />
  );
}

export default SeriesIndexTable;
