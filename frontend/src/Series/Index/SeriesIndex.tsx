import React, { useCallback, useMemo, useRef, useState } from 'react';
import QueueDetailsProvider from 'Activity/Queue/Details/QueueDetailsProvider';
import { useAppDimension } from 'App/appStore';
import { SelectProvider } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar, { PageJumpBarItems } from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import withScrollPosition from 'Components/withScrollPosition';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { align, icons, kinds } from 'Helpers/Props';
import { DESCENDING } from 'Helpers/Props/sortDirections';
import ParseToolbarButton from 'Parse/ParseToolbarButton';
import NoSeries from 'Series/NoSeries';
import {
  setSeriesOption,
  setSeriesSort,
  setSeriesTableOptions,
  useSeriesOptions,
} from 'Series/seriesOptionsStore';
import { FILTERS, useSeriesIndex } from 'Series/useSeries';
import scrollPositions from 'Store/scrollPositions';
import { TableOptionsChangePayload } from 'typings/Table';
import translate from 'Utilities/String/translate';
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
import SeriesIndexRefreshSeriesButton from './SeriesIndexRefreshSeriesButton';
import SeriesIndexTable from './Table/SeriesIndexTable';
import SeriesIndexTableOptions from './Table/SeriesIndexTableOptions';
import styles from './SeriesIndex.css';

function getViewComponent(view: string) {
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
    isLoading: isFetching,
    isFetched,
    isError: error,
    data,
    totalItems,
  } = useSeriesIndex();

  const { selectedFilterKey, sortKey, sortDirection, view, columns } =
    useSeriesOptions();
  const filters = FILTERS;

  const customFilters = useCustomFiltersList('series');

  const executeCommand = useExecuteCommand();
  const isRssSyncExecuting = useCommandExecuting(CommandNames.RssSync);
  const isSmallScreen = useAppDimension('isSmallScreen');
  const scrollerRef = useRef<HTMLDivElement>(null);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [jumpToCharacter, setJumpToCharacter] = useState<string | undefined>(
    undefined
  );
  const [isSelectMode, setIsSelectMode] = useState(false);

  const onRssSyncPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RssSync,
    });
  }, [executeCommand]);

  const onSelectModePress = useCallback(() => {
    setIsSelectMode(!isSelectMode);
  }, [isSelectMode, setIsSelectMode]);

  const onTableOptionChange = useCallback(
    (
      payload: TableOptionsChangePayload & {
        tableOptions?: { showBanners?: boolean; showSearchAction?: boolean };
      }
    ) => {
      if (payload.tableOptions) {
        setSeriesTableOptions(payload.tableOptions);
      } else if (payload.columns) {
        setSeriesOption('columns', payload.columns);
      }
    },
    []
  );

  const onViewSelect = useCallback(
    (value: string) => {
      setSeriesOption('view', value);

      if (scrollerRef.current) {
        scrollerRef.current.scrollTo(0, 0);
      }
    },
    [scrollerRef]
  );

  const onSortSelect = useCallback((value: string) => {
    setSeriesSort({ sortKey: value });
  }, []);

  const onFilterSelect = useCallback((value: string | number) => {
    setSeriesOption('selectedFilterKey', value);
  }, []);

  const onOptionsPress = useCallback(() => {
    setIsOptionsModalOpen(true);
  }, [setIsOptionsModalOpen]);

  const onOptionsModalClose = useCallback(() => {
    setIsOptionsModalOpen(false);
  }, [setIsOptionsModalOpen]);

  const onJumpBarItemPress = useCallback(
    (character: string) => {
      setJumpToCharacter(character);
    },
    [setJumpToCharacter]
  );

  const onScroll = useCallback(
    ({ scrollTop }: { scrollTop: number }) => {
      setJumpToCharacter(undefined);
      scrollPositions.seriesIndex = scrollTop;
    },
    [setJumpToCharacter]
  );

  const jumpBarItems: PageJumpBarItems = useMemo(() => {
    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      return {
        characters: {},
        order: [],
      };
    }

    const characters = data.reduce((acc: Record<string, number>, item) => {
      let char = item.sortTitle.charAt(0);

      if (!isNaN(Number(char))) {
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
    if (sortDirection === DESCENDING) {
      order.reverse();
    }

    return {
      characters,
      order,
    };
  }, [data, sortKey, sortDirection]);
  const ViewComponent = useMemo(() => getViewComponent(view), [view]);

  const isLoaded = !!(!error && isFetched && data.length);
  const hasNoSeries = !totalItems;

  return (
    <QueueDetailsProvider all={true}>
      <SelectProvider items={data}>
        <PageContent>
          <PageToolbar>
            <PageToolbarSection>
              <SeriesIndexRefreshSeriesButton
                isSelectMode={isSelectMode}
                selectedFilterKey={selectedFilterKey}
              />

              <PageToolbarButton
                label={translate('RssSync')}
                iconName={icons.RSS}
                isSpinning={isRssSyncExecuting}
                isDisabled={hasNoSeries}
                onPress={onRssSyncPress}
              />

              <PageToolbarSeparator />

              <SeriesIndexSelectModeButton
                label={
                  isSelectMode
                    ? translate('StopSelecting')
                    : translate('SelectSeries')
                }
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

              <PageToolbarSeparator />
              <ParseToolbarButton />
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
                  <PageToolbarButton
                    label={translate('Options')}
                    iconName={icons.TABLE}
                  />
                </TableOptionsModalWrapper>
              ) : (
                <PageToolbarButton
                  label={translate('Options')}
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
              // eslint-disable-next-line @typescript-eslint/ban-ts-comment
              // @ts-ignore
              innerClassName={styles[`${view}InnerContentBody`]}
              initialScrollTop={props.initialScrollTop}
              onScroll={onScroll}
            >
              {isFetching && !isFetched ? <LoadingIndicator /> : null}

              {!isFetching && !!error ? (
                <Alert kind={kinds.DANGER}>
                  {translate('SeriesLoadError')}
                </Alert>
              ) : null}

              {isLoaded ? (
                <div className={styles.contentBodyContainer}>
                  <ViewComponent
                    scrollerRef={scrollerRef}
                    items={data}
                    sortKey={sortKey}
                    sortDirection={sortDirection}
                    jumpToCharacter={jumpToCharacter}
                    isSelectMode={isSelectMode}
                    isSmallScreen={isSmallScreen}
                  />

                  <SeriesIndexFooter />
                </div>
              ) : null}

              {!error && isFetched && !data.length ? (
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
    </QueueDetailsProvider>
  );
}, 'seriesIndex');

export default SeriesIndex;
