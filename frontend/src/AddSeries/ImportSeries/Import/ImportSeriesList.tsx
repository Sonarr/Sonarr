import { throttle } from 'lodash';
import React, {
  RefObject,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { ListChildComponentProps, VariableSizeList } from 'react-window';
import { useAppDimensions } from 'App/appStore';
import { useSelect } from 'App/Select/SelectContext';
import Scroller from 'Components/Scroller/Scroller';
import useMeasure from 'Helpers/Hooks/useMeasure';
import useSeries from 'Series/useSeries';
import ImportSeriesCard from './ImportSeriesCard';
import {
  ImportSeriesItem,
  isReadyToImport,
  UnamppedFolderItem,
  useEnsureImportSeriesItems,
  useImportSeriesItems,
  useImportSeriesViewOption,
} from './importSeriesStore';
import styles from './ImportSeriesList.css';

const ITEM_HEIGHT = 112;
const ITEM_HEIGHT_COMPACT = 64;
const ITEM_HEIGHT_STACKED = 203;
const ITEM_HEIGHT_STACKED_UNMATCHED = 80;
const STACKED_WIDTH = 960;

interface Row {
  isReady: boolean;
  item: UnamppedFolderItem;
}

interface ItemData {
  rows: Row[];
  isCompact: boolean;
  isStacked: boolean;
}

interface ImportSeriesListProps {
  items: UnamppedFolderItem[];
  scrollerRef: RefObject<HTMLElement>;
}

function getWindowScrollTopPosition() {
  return document.documentElement.scrollTop || document.body.scrollTop || 0;
}

function Item({ index, style, data }: ListChildComponentProps<ItemData>) {
  const { rows, isCompact, isStacked } = data;
  const row = rows[index];

  if (!row) {
    return null;
  }

  return (
    <div style={style} className={styles.item}>
      <ImportSeriesCard
        unmappedFolder={row.item}
        isCompact={isCompact}
        isStacked={isStacked}
      />
    </div>
  );
}

function ImportSeriesList({ items, scrollerRef }: ImportSeriesListProps) {
  const {
    width: windowWidth,
    height: windowHeight,
    isSmallScreen,
  } = useAppDimensions();
  const { useHasItems, toggleDisabled, toggleSelected } =
    useSelect<ImportSeriesItem>();
  const compactRows = useImportSeriesViewOption('compactRows');
  const importSeriesItems = useImportSeriesItems();
  const { data: existingSeries = [] } = useSeries();

  const listRef = useRef<VariableSizeList<ItemData>>(null);
  const readyIdsRef = useRef(new Set<string>());
  const [measureRef, bounds] = useMeasure();
  const [size, setSize] = useState({ width: 0, height: 0 });

  const hasSelectItems = useHasItems();

  useEnsureImportSeriesItems(items);

  const importSeriesItemById = useMemo(() => {
    return new Map(importSeriesItems.map((item) => [item.id, item]));
  }, [importSeriesItems]);

  const existingTvdbIds = useMemo(() => {
    return new Set(existingSeries.map((series) => series.tvdbId));
  }, [existingSeries]);

  const rows = useMemo(() => {
    return items.map((item) => ({
      item,
      isReady: isReadyToImport(
        importSeriesItemById.get(item.id),
        existingTvdbIds
      ),
    }));
  }, [items, importSeriesItemById, existingTvdbIds]);

  useEffect(() => {
    const nextReadyIds = new Set<string>();

    rows.forEach(({ item, isReady }) => {
      toggleDisabled(item.id, !isReady);

      if (isReady) {
        nextReadyIds.add(item.id);

        if (!readyIdsRef.current.has(item.id)) {
          toggleSelected({ id: item.id, isSelected: true, shiftKey: false });
        }
      }
    });

    readyIdsRef.current = nextReadyIds;
  }, [rows, toggleDisabled, toggleSelected]);

  const isStacked = size.width > 0 && size.width < STACKED_WIDTH;
  const isCompact = compactRows && !isStacked;

  const getItemSize = useCallback(
    (index: number) => {
      if (isCompact) {
        return ITEM_HEIGHT_COMPACT;
      }

      if (isStacked) {
        return rows[index]?.isReady
          ? ITEM_HEIGHT_STACKED
          : ITEM_HEIGHT_STACKED_UNMATCHED;
      }

      return ITEM_HEIGHT;
    },
    [rows, isCompact, isStacked]
  );

  const getItemKey = useCallback(
    (index: number, data: ItemData) => data.rows[index]?.item.id ?? index,
    []
  );

  useEffect(() => {
    listRef.current?.resetAfterIndex(0, true);
  }, [rows, isCompact, isStacked]);

  useEffect(() => {
    if (isSmallScreen) {
      setSize({
        width: windowWidth,
        height: windowHeight,
      });

      return;
    }

    if (bounds.width) {
      setSize({
        width: bounds.width,
        height: windowHeight,
      });
    }
  }, [isSmallScreen, windowWidth, windowHeight, bounds]);

  useEffect(() => {
    const currentScrollerRef = scrollerRef.current as HTMLElement;
    const currentScrollListener = isSmallScreen ? window : currentScrollerRef;

    const handleScroll = throttle(() => {
      const { offsetTop = 0 } = currentScrollerRef;
      const scrollTop =
        (isSmallScreen
          ? getWindowScrollTopPosition()
          : currentScrollerRef.scrollTop) - offsetTop;

      listRef.current?.scrollTo(scrollTop);
    }, 10);

    currentScrollListener.addEventListener('scroll', handleScroll);

    return () => {
      handleScroll.cancel();

      if (currentScrollListener) {
        currentScrollListener.removeEventListener('scroll', handleScroll);
      }
    };
  }, [isSmallScreen, listRef, scrollerRef]);

  if (!items.length || !hasSelectItems) {
    return null;
  }

  return (
    <div ref={measureRef}>
      <Scroller className={styles.listScroller} scrollDirection="horizontal">
        <VariableSizeList<ItemData>
          ref={listRef}
          style={{
            width: '100%',
            height: '100%',
            overflow: 'none',
          }}
          width={size.width}
          height={size.height}
          itemCount={rows.length}
          itemKey={getItemKey}
          itemSize={getItemSize}
          estimatedItemSize={isStacked ? ITEM_HEIGHT_STACKED : ITEM_HEIGHT}
          itemData={{
            rows,
            isCompact,
            isStacked,
          }}
          overscanCount={20}
        >
          {Item}
        </VariableSizeList>
      </Scroller>
    </div>
  );
}

export default ImportSeriesList;
