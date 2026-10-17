import { throttle } from 'lodash';
import React, {
  RefObject,
  useCallback,
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { ListChildComponentProps, VariableSizeList } from 'react-window';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useMeasure from 'Helpers/Hooks/useMeasure';
import { kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import InteractiveSearchRow from './InteractiveSearchRow';
import InteractiveSearchTableHeader from './InteractiveSearchTableHeader';
import { useReleaseOptions } from './releaseOptionsStore';
import useReleases, { Release, setReleaseSort } from './useReleases';
import styles from './InteractiveSearch.module.css';

const ESTIMATED_ROW_HEIGHT = 35;

interface RowItemData {
  items: Release[];
  searchPayload: InteractiveSearchPayload;
  setRowHeight: (index: number, height: number) => void;
}

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items, searchPayload, setRowHeight } = data;

  if (index >= items.length) {
    return null;
  }

  const item = items[index];

  return (
    <InteractiveSearchRow
      key={`${item.release.indexerId}-${item.release.guid}`}
      index={index}
      style={style}
      setRowHeight={setRowHeight}
      {...item}
      searchPayload={searchPayload}
    />
  );
}

interface InteractiveSearchProps {
  searchPayload: InteractiveSearchPayload;
  scrollerRef: RefObject<HTMLDivElement>;
  filter: string;
}

function InteractiveSearch({
  searchPayload,
  scrollerRef,
  filter,
}: InteractiveSearchProps) {
  const { columns } = useReleaseOptions();

  const {
    isFetching,
    isFetched,
    error,
    data,
    totalItems,
    sortKey,
    sortDirection,
  } = useReleases(searchPayload, { filter });

  const listRef = useRef<VariableSizeList<RowItemData>>(null);
  const listOuterRef = useRef<HTMLDivElement>(null);
  const tableContainerRef = useRef<HTMLDivElement>(null);
  const scrollbarRef = useRef<HTMLDivElement>(null);
  const rowHeights = useRef<number[]>([]);
  const [measureRef, bounds] = useMeasure();
  const [viewportHeight, setViewportHeight] = useState(0);

  const setRowHeight = useCallback((index: number, height: number) => {
    if (rowHeights.current[index] === height) {
      return;
    }

    rowHeights.current[index] = height;
    listRef.current?.resetAfterIndex(index);
  }, []);

  const getRowHeight = useCallback((index: number) => {
    return rowHeights.current[index] ?? ESTIMATED_ROW_HEIGHT;
  }, []);

  useLayoutEffect(() => {
    listRef.current?.resetAfterIndex(0);
  }, [data]);

  useEffect(() => {
    const scroller = scrollerRef.current;

    if (!scroller) {
      return;
    }

    const updateHeight = () => setViewportHeight(scroller.clientHeight);

    updateHeight();

    const observer = new ResizeObserver(updateHeight);
    observer.observe(scroller);

    return () => observer.disconnect();
  }, [scrollerRef]);

  useEffect(() => {
    const scroller = scrollerRef.current;

    if (!scroller) {
      return;
    }

    const handleScroll = throttle(() => {
      const outer = listOuterRef.current;

      if (!outer) {
        return;
      }

      const scrolled =
        scroller.getBoundingClientRect().top -
        outer.getBoundingClientRect().top;

      listRef.current?.scrollTo(Math.max(0, scrolled));
    }, 10);

    scroller.addEventListener('scroll', handleScroll);

    return () => {
      handleScroll.cancel();
      scroller.removeEventListener('scroll', handleScroll);
    };
  }, [scrollerRef]);

  const itemData = useMemo<RowItemData>(
    () => ({ items: data, searchPayload, setRowHeight }),
    [data, searchPayload, setRowHeight]
  );

  const handleTableScroll = useCallback(() => {
    const tableContainer = tableContainerRef.current;
    const scrollbar = scrollbarRef.current;

    if (tableContainer && scrollbar) {
      scrollbar.scrollLeft = tableContainer.scrollLeft;
    }
  }, []);

  const handleScrollbarScroll = useCallback(() => {
    const tableContainer = tableContainerRef.current;
    const scrollbar = scrollbarRef.current;

    if (tableContainer && scrollbar) {
      tableContainer.scrollLeft = scrollbar.scrollLeft;
    }
  }, []);

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setReleaseSort(sortKey, sortDirection);
    },
    []
  );

  const errorMessage = getErrorMessage(error);

  return (
    <div>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <div>
          {errorMessage ? (
            <>
              {translate('InteractiveSearchResultsSeriesFailedErrorMessage', {
                message:
                  errorMessage.charAt(0).toLowerCase() + errorMessage.slice(1),
              })}
            </>
          ) : (
            translate('EpisodeSearchResultsLoadError')
          )}
        </div>
      ) : null}

      {!isFetching && isFetched && !totalItems ? (
        <Alert kind={kinds.INFO}>{translate('NoResultsFound')}</Alert>
      ) : null}

      {!!totalItems && !isFetching && !data.length ? (
        <Alert kind={kinds.WARNING}>
          {translate('AllResultsAreHiddenByTheAppliedFilter')}
        </Alert>
      ) : null}

      {!isFetching && !!data.length ? (
        <div
          ref={tableContainerRef}
          className={styles.tableContainer}
          onScroll={handleTableScroll}
        >
          <div ref={measureRef} className={styles.table}>
            <InteractiveSearchTableHeader
              columns={columns}
              sortKey={sortKey}
              sortDirection={sortDirection}
              onSortPress={handleSortPress}
            />

            <VariableSizeList<RowItemData>
              ref={listRef}
              outerRef={listOuterRef}
              style={{ width: '100%', height: '100%', overflow: 'visible' }}
              width={bounds.width}
              height={viewportHeight}
              itemCount={data.length}
              itemSize={getRowHeight}
              estimatedItemSize={ESTIMATED_ROW_HEIGHT}
              itemData={itemData}
              overscanCount={20}
            >
              {Row}
            </VariableSizeList>
          </div>
        </div>
      ) : null}

      {!isFetching && !!data.length ? (
        <div
          ref={scrollbarRef}
          className={styles.scrollbar}
          onScroll={handleScrollbarScroll}
        >
          <div
            className={styles.scrollbarContent}
            style={{ width: bounds.width }}
          />
        </div>
      ) : null}

      {!isFetching && totalItems !== data.length && !!data.length ? (
        <div className={styles.filteredMessage}>
          {translate('SomeResultsAreHiddenByTheAppliedFilter')}
        </div>
      ) : null}
    </div>
  );
}

export default InteractiveSearch;
