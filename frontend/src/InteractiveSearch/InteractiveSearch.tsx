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
import TextInput from 'Components/Form/TextInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useMeasure from 'Helpers/Hooks/useMeasure';
import { align, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import InteractiveSearchFilterModal from './InteractiveSearchFilterModal';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import InteractiveSearchRow from './InteractiveSearchRow';
import InteractiveSearchTableHeader from './InteractiveSearchTableHeader';
import InteractiveSearchType from './InteractiveSearchType';
import { setReleaseOption, useReleaseOptions } from './releaseOptionsStore';
import useReleases, { FILTERS, Release, setReleaseSort } from './useReleases';
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
  type: InteractiveSearchType;
  searchPayload: InteractiveSearchPayload;
  scrollerRef: RefObject<HTMLDivElement>;
}

function InteractiveSearch({
  type,
  searchPayload,
  scrollerRef,
}: InteractiveSearchProps) {
  const customFilters = useCustomFiltersList('releases');
  const { columns } = useReleaseOptions();

  const [filter, setFilter] = useState('');

  const {
    isFetching,
    isFetched,
    error,
    data,
    totalItems,
    selectedFilterKey,
    sortKey,
    sortDirection,
  } = useReleases(searchPayload, filter);

  const listRef = useRef<VariableSizeList<RowItemData>>(null);
  const listOuterRef = useRef<HTMLDivElement>(null);
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

  const onFilterChange = useCallback(({ value }: InputChanged<string>) => {
    setFilter(value);
  }, []);

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      if (type === 'episode') {
        setReleaseOption('episodeSelectedFilterKey', selectedFilterKey);
      } else {
        setReleaseOption('seasonSelectedFilterKey', selectedFilterKey);
      }
    },
    [type]
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setReleaseSort(sortKey, sortDirection);
    },
    []
  );

  const errorMessage = getErrorMessage(error);

  return (
    <div>
      <div className={styles.filterRow}>
        <div className={styles.filterInput}>
          <TextInput
            name="releaseFilter"
            value={filter}
            placeholder={translate('FilterReleasesPlaceholder')}
            onChange={onFilterChange}
          />
        </div>

        <FilterMenu
          alignMenu={align.RIGHT}
          selectedFilterKey={selectedFilterKey}
          filters={FILTERS}
          customFilters={customFilters}
          buttonComponent={PageMenuButton}
          filterModalConnectorComponent={InteractiveSearchFilterModal}
          filterModalConnectorComponentProps={{ type, searchPayload }}
          onFilterSelect={handleFilterSelect}
        />
      </div>

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
        <div ref={measureRef}>
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
