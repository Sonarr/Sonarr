import React, { useCallback, useMemo, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { SelectProvider } from 'App/SelectContext';
import { REFRESH_SERIES, RSS_SYNC } from 'Commands/commandNames';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import withScrollPosition from 'Components/withScrollPosition';
import { align, icons } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import NoSeries from 'Series/NoSeries';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  setSeriesFilter,
  setSeriesSort,
  setSeriesTableOption,
  setSeriesView,
} from 'Store/Actions/seriesIndexActions';
import scrollPositions from 'Store/scrollPositions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSeriesClientSideCollectionItemsSelector from 'Store/Selectors/createSeriesClientSideCollectionItemsSelector';
import SeriesIndexFilterMenu from './Menus/SeriesIndexFilterMenu';
import SeriesIndexSortMenu from './Menus/SeriesIndexSortMenu';
import SeriesIndexViewMenu from './Menus/SeriesIndexViewMenu';
import SeriesIndexOverviewOptionsModal from './Overview/Options/SeriesIndexOverviewOptionsModal';
import SeriesIndexOverviews from './Overview/SeriesIndexOverviews';
import SeriesIndexPosterOptionsModal from './Posters/Options/SeriesIndexPosterOptionsModal';
import SeriesIndexPosters from './Posters/SeriesIndexPosters';
import SeriesIndexSelectAllButton from './Select/SeriesIndexSelectAllButton';
import SeriesIndexSelectAllMenuItem from './Select/SeriesIndexSelectAllMenuItem';
import SeriesIndexSelectFooter from './Select/SeriesIndexSelectFooter';
import SeriesIndexSelectModeButton from './Select/SeriesIndexSelectModeButton';
import SeriesIndexSelectModeMenuItem from './Select/SeriesIndexSelectModeMenuItem';
import SeriesIndexFooter from './SeriesIndexFooter';
import SeriesIndexTable from './Table/SeriesIndexTable';
import SeriesIndexTableOptions from './Table/SeriesIndexTableOptions';
import styles from './SeriesIndex.css';

function getViewComponent(view) {
  if (view === 'posters') {
    return SeriesIndexPosters;
  }

  if (view === 'overview') {
    return SeriesIndexOverviews;
  }

  return SeriesIndexTable;
}

interface SeriesIndexProps {
  initialScrollTop?: number;
}

const SeriesIndex = withScrollPosition((props: SeriesIndexProps) => {
  const {
    isFetching,
    isPopulated,
    error,
    totalItems,
    items,
    columns,
    selectedFilterKey,
    filters,
    customFilters,
    sortKey,
    sortDirection,
    view,
  } = useSelector(createSeriesClientSideCollectionItemsSelector('seriesIndex'));

  const isRefreshingSeries = useSelector(
    createCommandExecutingSelector(REFRESH_SERIES)
  );
  const isRssSyncExecuting = useSelector(
    createCommandExecutingSelector(RSS_SYNC)
  );
  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const dispatch = useDispatch();
  const scrollerRef = useRef<HTMLDivElement>();
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [jumpToCharacter, setJumpToCharacter] = useState<string | null>(null);
  const [isSelectMode, setIsSelectMode] = useState(false);

  const onRefreshSeriesPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_SERIES,
      })
    );
  }, [dispatch]);

  const onRssSyncPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: RSS_SYNC,
      })
    );
  }, [dispatch]);

  const onSelectModePress = useCallback(() => {
    setIsSelectMode(!isSelectMode);
  }, [isSelectMode, setIsSelectMode]);

  const onTableOptionChange = useCallback(
    (payload) => {
      dispatch(setSeriesTableOption(payload));
    },
    [dispatch]
  );

  const onViewSelect = useCallback(
    (value) => {
      dispatch(setSeriesView({ view: value }));

      if (scrollerRef.current) {
        scrollerRef.current.scrollTo(0, 0);
      }
    },
    [scrollerRef, dispatch]
  );

  const onSortSelect = useCallback(
    (value) => {
      dispatch(setSeriesSort({ sortKey: value }));
    },
    [dispatch]
  );

  const onFilterSelect = useCallback(
    (value) => {
      dispatch(setSeriesFilter({ selectedFilterKey: value }));
    },
    [dispatch]
  );

  const onOptionsPress = useCallback(() => {
    setIsOptionsModalOpen(true);
  }, [setIsOptionsModalOpen]);

  const onOptionsModalClose = useCallback(() => {
    setIsOptionsModalOpen(false);
  }, [setIsOptionsModalOpen]);

  const onJumpBarItemPress = useCallback(
    (character) => {
      setJumpToCharacter(character);
    },
    [setJumpToCharacter]
  );

  const onScroll = useCallback(
    ({ scrollTop }) => {
      setJumpToCharacter(null);
      scrollPositions.seriesIndex = scrollTop;
    },
    [setJumpToCharacter]
  );

  const jumpBarItems = useMemo(() => {
    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      return {
        order: [],
      };
    }

    const characters = items.reduce((acc, item) => {
      let char = item.sortTitle.charAt(0);

      if (!isNaN(char)) {
        char = '#';
      }

      if (char in acc) {
        acc[char] = acc[char] + 1;
      } else {
        acc[char] = 1;
      }

      return acc;
    }, {});

    const order = Object.keys(characters).sort();

    // Reverse if sorting descending
    if (sortDirection === SortDirection.Descending) {
      order.reverse();
    }

    return {
      characters,
      order,
    };
  }, [items, sortKey, sortDirection]);
  const ViewComponent = useMemo(() => getViewComponent(view), [view]);

  const isLoaded = !!(!error && isPopulated && items.length);
  const hasNoSeries = !totalItems;

  return (
    <SelectProvider items={items}>
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label="Update all"
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              isSpinning={isRefreshingSeries}
              isDisabled={hasNoSeries}
              onPress={onRefreshSeriesPress}
            />

            <PageToolbarButton
              label="RSS Sync"
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              isDisabled={hasNoSeries}
              onPress={onRssSyncPress}
            />

            <PageToolbarSeparator />

            <SeriesIndexSelectModeButton
              label={isSelectMode ? 'Stop Selecting' : 'Select Series'}
              iconName={isSelectMode ? icons.SERIES_ENDED : icons.CHECK}
              isSelectMode={isSelectMode}
              overflowComponent={SeriesIndexSelectModeMenuItem}
              onPress={onSelectModePress}
            />

            <SeriesIndexSelectAllButton
              label="SelectAll"
              isSelectMode={isSelectMode}
              overflowComponent={SeriesIndexSelectAllMenuItem}
            />
          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >
            {view === 'table' ? (
              <TableOptionsModalWrapper
                columns={columns}
                optionsComponent={SeriesIndexTableOptions}
                onTableOptionChange={onTableOptionChange}
              >
                <PageToolbarButton label="Options" iconName={icons.TABLE} />
              </TableOptionsModalWrapper>
            ) : (
              <PageToolbarButton
                label="Options"
                iconName={view === 'posters' ? icons.POSTER : icons.OVERVIEW}
                isDisabled={hasNoSeries}
                onPress={onOptionsPress}
              />
            )}

            <PageToolbarSeparator />

            <SeriesIndexViewMenu
              view={view}
              isDisabled={hasNoSeries}
              onViewSelect={onViewSelect}
            />

            <SeriesIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoSeries}
              onSortSelect={onSortSelect}
            />

            <SeriesIndexFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoSeries}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>
        <div className={styles.pageContentBodyWrapper}>
          <PageContentBody
            ref={scrollerRef}
            className={styles.contentBody}
            innerClassName={styles[`${view}InnerContentBody`]}
            initialScrollTop={props.initialScrollTop}
            onScroll={onScroll}
          >
            {isFetching && !isPopulated ? <LoadingIndicator /> : null}

            {!isFetching && !!error ? <div>Unable to load series</div> : null}

            {isLoaded ? (
              <div className={styles.contentBodyContainer}>
                <ViewComponent
                  scrollerRef={scrollerRef}
                  items={items}
                  sortKey={sortKey}
                  sortDirection={sortDirection}
                  jumpToCharacter={jumpToCharacter}
                  isSelectMode={isSelectMode}
                  isSmallScreen={isSmallScreen}
                />

                <SeriesIndexFooter />
              </div>
            ) : null}

            {!error && isPopulated && !items.length ? (
              <NoSeries totalItems={totalItems} />
            ) : null}
          </PageContentBody>
          {isLoaded && !!jumpBarItems.order.length ? (
            <PageJumpBar
              items={jumpBarItems}
              onItemPress={onJumpBarItemPress}
            />
          ) : null}
        </div>

        {isSelectMode ? <SeriesIndexSelectFooter /> : null}

        {view === 'posters' ? (
          <SeriesIndexPosterOptionsModal
            isOpen={isOptionsModalOpen}
            onModalClose={onOptionsModalClose}
          />
        ) : null}
        {view === 'overview' ? (
          <SeriesIndexOverviewOptionsModal
            isOpen={isOptionsModalOpen}
            onModalClose={onOptionsModalClose}
          />
        ) : null}
      </PageContent>
    </SelectProvider>
  );
}, 'seriesIndex');

export default SeriesIndex;
