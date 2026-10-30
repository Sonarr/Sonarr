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
import { COLUMN_STYLE, ROW_LAYOUT_MIN_WIDTH } from './InteractiveSearchColumns';
import InteractiveSearchColumnsHeader from './InteractiveSearchColumnsHeader';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import InteractiveSearchResult from './InteractiveSearchResult';
import useReleases, {
  FIXED_SORT_DIRECTIONS,
  Release,
  setReleaseSort,
} from './useReleases';
import styles from './InteractiveSearch.module.css';

const ESTIMATED_CARD_HEIGHT = 110;
const ESTIMATED_ROW_HEIGHT = 85;

interface RowItemData {
  items: Release[];
  searchPayload: InteractiveSearchPayload;
  setRowHeight: (index: number, height: number) => void;
  isRowLayout: boolean;
}

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items, searchPayload, setRowHeight, isRowLayout } = data;

  if (index >= items.length) {
    return null;
  }

  const item = items[index];

  return (
    <InteractiveSearchResult
      key={`${item.release.indexerId}-${item.release.guid}`}
      index={index}
      style={style}
      isRowLayout={isRowLayout}
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
  const rowHeights = useRef<number[]>([]);
  const [measureRef, bounds] = useMeasure();
  const [viewportHeight, setViewportHeight] = useState(0);

  const isRowLayout = bounds.width >= ROW_LAYOUT_MIN_WIDTH;
  const measuredLayout = useRef(isRowLayout);

  if (measuredLayout.current !== isRowLayout) {
    measuredLayout.current = isRowLayout;
    rowHeights.current = [];
  }

  const estimatedHeight = isRowLayout
    ? ESTIMATED_ROW_HEIGHT
    : ESTIMATED_CARD_HEIGHT;

  const setRowHeight = useCallback((index: number, height: number) => {
    if (rowHeights.current[index] === height) {
      return;
    }

    rowHeights.current[index] = height;
    listRef.current?.resetAfterIndex(index);
  }, []);

  const getRowHeight = useCallback(
    (index: number) => {
      return rowHeights.current[index] ?? estimatedHeight;
    },
    [estimatedHeight]
  );

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

  useLayoutEffect(() => {
    listRef.current?.resetAfterIndex(0);
  }, [isRowLayout]);

  const itemData = useMemo<RowItemData>(
    () => ({ items: data, searchPayload, setRowHeight, isRowLayout }),
    [data, searchPayload, setRowHeight, isRowLayout]
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setReleaseSort(sortKey, sortDirection ?? FIXED_SORT_DIRECTIONS[sortKey]);
    },
    []
  );

  const errorMessage = getErrorMessage(error);

  return (
    <div ref={measureRef} style={COLUMN_STYLE}>
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

      {isRowLayout && !isFetching && !!data.length ? (
        <InteractiveSearchColumnsHeader
          sortKey={sortKey}
          sortDirection={sortDirection}
          onSortPress={handleSortPress}
        />
      ) : null}

      {!isFetching && !!data.length ? (
        <div className={styles.results}>
          <VariableSizeList<RowItemData>
            ref={listRef}
            outerRef={listOuterRef}
            style={{ width: '100%', height: '100%', overflow: 'visible' }}
            width={bounds.width}
            height={viewportHeight}
            itemCount={data.length}
            itemSize={getRowHeight}
            estimatedItemSize={estimatedHeight}
            itemData={itemData}
            overscanCount={20}
          >
            {Row}
          </VariableSizeList>
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
