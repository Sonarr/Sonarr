import React, { useCallback, useMemo, useRef, useState } from 'react';
import QueueDetailsProvider from 'Activity/Queue/Details/QueueDetailsProvider';
import { useAppDimension } from 'App/appStore';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar, { PageJumpBarItems } from 'Components/Page/PageJumpBar';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import { PageToolbarButtonProps } from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import TableOptionsModal from 'Components/Table/TableOptions/TableOptionsModal';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { icons, kinds } from 'Helpers/Props';
import { DESCENDING } from 'Helpers/Props/sortDirections';
import ParseModal from 'Parse/ParseModal';
import NoSeries from 'Series/NoSeries';
import Series from 'Series/Series';
import {
  setSeriesOption,
  setSeriesSort,
  setSeriesTableOptions,
  useSeriesOptions,
} from 'Series/seriesOptionsStore';
import { FILTERS, useSeriesIndex } from 'Series/useSeries';
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
import SeriesIndexTable from './Table/SeriesIndexTable';
import SeriesIndexTableOptions from './Table/SeriesIndexTableOptions';
import styles from './SeriesIndex.css';

function getViewComponent(view: string) {
  if (view === 'posters') return SeriesIndexPosters;
  if (view === 'overview') return SeriesIndexOverviews;

  return SeriesIndexTable;
}

function getOptionsIcon(view: string) {
  if (view === 'posters') return icons.POSTER;
  if (view === 'overview') return icons.OVERVIEW;

  return icons.TABLE;
}

function SeriesIndex() {
  const seriesIndex = useSeriesIndex();

  return (
    <QueueDetailsProvider all={true}>
      <SelectProvider items={seriesIndex.data}>
        <SeriesIndexBody seriesIndex={seriesIndex} />
      </SelectProvider>
    </QueueDetailsProvider>
  );
}

interface SeriesIndexBodyProps {
  seriesIndex: ReturnType<typeof useSeriesIndex>;
}

function SeriesIndexBody({ seriesIndex }: SeriesIndexBodyProps) {
  const {
    isLoading: isFetching,
    isFetched,
    isError: error,
    data,
    totalItems,
  } = seriesIndex;

  const { selectedFilterKey, sortKey, sortDirection, view, columns } =
    useSeriesOptions();
  const filters = FILTERS;

  const customFilters = useCustomFiltersList('series');

  const executeCommand = useExecuteCommand();
  const isRssSyncExecuting = useCommandExecuting(CommandNames.RssSync);
  const isRefreshingSeries = useCommandExecuting(CommandNames.RefreshSeries);
  const isSmallScreen = useAppDimension('isSmallScreen');
  const scrollerRef = useRef<HTMLDivElement>(null);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [isParseModalOpen, setIsParseModalOpen] = useState(false);
  const [jumpToCharacter, setJumpToCharacter] = useState<string | undefined>(
    undefined
  );
  const [isSelectMode, setIsSelectMode] = useState(false);
  const { anySelected, getSelectedIds } = useSelect<Series>();

  let refreshLabel = translate('UpdateAll');

  if (anySelected) {
    refreshLabel = translate('UpdateSelected');
  } else if (selectedFilterKey !== 'all') {
    refreshLabel = translate('UpdateFiltered');
  }

  const handleRefreshSeriesPress = useCallback(() => {
    const seriesToRefresh =
      isSelectMode && anySelected ? getSelectedIds() : data.map((m) => m.id);

    executeCommand({
      name: CommandNames.RefreshSeries,
      seriesIds: seriesToRefresh,
    });
  }, [executeCommand, anySelected, isSelectMode, data, getSelectedIds]);

  const onRssSyncPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RssSync,
    });
  }, [executeCommand]);

  const onSelectModePress = useCallback(() => {
    setIsSelectMode((val) => !val);
  }, []);

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

  const onOptionsPress = useCallback(
    () => setIsOptionsModalOpen(true),
    [setIsOptionsModalOpen]
  );

  const onOptionsModalClose = useCallback(
    () => setIsOptionsModalOpen(false),
    [setIsOptionsModalOpen]
  );

  const onJumpBarItemPress = useCallback(
    (character: string) => {
      setJumpToCharacter(character);
    },
    [setJumpToCharacter]
  );

  const onScroll = useCallback(() => {
    setJumpToCharacter(undefined);
  }, [setJumpToCharacter]);

  const [tableOptionsModalOpen, setTableOptionsModalOpen] = useState(false);

  const handleTableOptionsPress = useCallback(
    () => setTableOptionsModalOpen(true),
    []
  );

  const handleTableOptionsModalClose = useCallback(
    () => setTableOptionsModalOpen(false),
    []
  );

  const handleParseModalPress = useCallback(
    () => setIsParseModalOpen(true),
    []
  );

  const handleParseModalClose = useCallback(
    () => setIsParseModalOpen(false),
    []
  );

  const handleOptionsTrigger = useCallback(() => {
    if (view === 'table') {
      handleTableOptionsPress();
    } else {
      onOptionsPress();
    }
  }, [view, handleTableOptionsPress, onOptionsPress]);

  const isLoaded = !!(!error && isFetched && data.length);
  const hasNoSeries = !totalItems;

  const renderSelectButton = useCallback(
    ({ label, iconName }: PageToolbarButtonProps) => (
      <SeriesIndexSelectModeButton
        label={label}
        iconName={iconName}
        isSelectMode={isSelectMode}
        onPress={onSelectModePress}
      />
    ),
    [isSelectMode, onSelectModePress]
  );

  const renderSelectOverflow = useCallback(
    ({ label, iconName }: PageToolbarButtonProps) => (
      <SeriesIndexSelectModeMenuItem
        label={label}
        iconName={iconName}
        isSelectMode={isSelectMode}
        onPress={onSelectModePress}
      />
    ),
    [isSelectMode, onSelectModePress]
  );

  const renderSelectAllButton = useCallback(
    () => <SeriesIndexSelectAllButton />,
    []
  );

  const renderSelectAllOverflow = useCallback(
    ({ label }: PageToolbarButtonProps) => (
      <SeriesIndexSelectAllMenuItem label={label} isSelectMode={isSelectMode} />
    ),
    [isSelectMode]
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

  return (
    <PageContent>
      <PageToolbar>
        <ToolbarItem
          id="refresh"
          priority={1}
          groupId="left-a"
          label={refreshLabel}
          iconName={icons.REFRESH}
          isSpinning={isRefreshingSeries}
          isDisabled={hasNoSeries}
          onPress={handleRefreshSeriesPress}
        />

        <ToolbarItem
          id="rss"
          priority={1}
          groupId="left-a"
          label={translate('RssSync')}
          iconName={icons.RSS}
          isSpinning={isRssSyncExecuting}
          isDisabled={hasNoSeries}
          onPress={onRssSyncPress}
        />

        <OverflowDivider groupId="left-a">
          <PageToolbarSeparator />
        </OverflowDivider>

        <ToolbarItem
          id="select"
          priority={1}
          groupId="left-b"
          label={
            isSelectMode
              ? translate('StopSelecting')
              : translate('SelectSeries')
          }
          iconName={isSelectMode ? icons.SERIES_ENDED : icons.CHECK}
          renderButton={renderSelectButton}
          renderOverflow={renderSelectOverflow}
          onPress={onSelectModePress}
        />

        {isSelectMode && (
          <ToolbarItem
            id="selectall"
            priority={1}
            groupId="left-b"
            label={translate('SelectAll')}
            iconName={icons.CHECK_SQUARE}
            renderButton={renderSelectAllButton}
            renderOverflow={renderSelectAllOverflow}
          />
        )}

        <OverflowDivider groupId="left-b">
          <PageToolbarSeparator />
        </OverflowDivider>

        <ToolbarItem
          id="parse"
          priority={1}
          groupId="left-c"
          label={translate('TestParsing')}
          iconName={icons.PARSE}
          onPress={handleParseModalPress}
        />

        <PageToolbarSpacer />

        <ToolbarItem
          id="options"
          priority={2}
          groupId="right"
          label={translate('Options')}
          iconName={getOptionsIcon(view)}
          isDisabled={hasNoSeries}
          onPress={handleOptionsTrigger}
        />

        <OverflowDivider groupId="right">
          <PageToolbarSeparator />
        </OverflowDivider>

        <ToolbarItem id="view" pinned={true}>
          <SeriesIndexViewMenu
            view={view}
            isDisabled={hasNoSeries}
            onViewSelect={onViewSelect}
          />
        </ToolbarItem>

        <ToolbarItem id="sort" pinned={true}>
          <SeriesIndexSortMenu
            sortKey={sortKey}
            sortDirection={sortDirection}
            isDisabled={hasNoSeries}
            onSortSelect={onSortSelect}
          />
        </ToolbarItem>

        <ToolbarItem id="filter" pinned={true}>
          <SeriesIndexFilterMenu
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={customFilters}
            isDisabled={hasNoSeries}
            onFilterSelect={onFilterSelect}
          />
        </ToolbarItem>
      </PageToolbar>
      <div className={styles.pageContentBodyWrapper}>
        <PageContentBody
          ref={scrollerRef}
          className={styles.contentBody}
          innerClassName={
            (styles as unknown as Record<string, string>)[
              `${view}InnerContentBody`
            ]
          }
          scrollPositionKey="seriesIndex"
          onScroll={onScroll}
        >
          {isFetching && !isFetched ? <LoadingIndicator /> : null}

          {!isFetching && !!error ? (
            <Alert kind={kinds.DANGER}>{translate('SeriesLoadError')}</Alert>
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
          <PageJumpBar items={jumpBarItems} onItemPress={onJumpBarItemPress} />
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

      <TableOptionsModal
        isOpen={tableOptionsModalOpen}
        columns={columns}
        optionsComponent={SeriesIndexTableOptions}
        onTableOptionChange={onTableOptionChange}
        onModalClose={handleTableOptionsModalClose}
      />

      <ParseModal
        isOpen={isParseModalOpen}
        onModalClose={handleParseModalClose}
      />
    </PageContent>
  );
}

export default SeriesIndex;
