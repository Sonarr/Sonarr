import React, { useCallback, useEffect, useMemo, useState } from 'react';
import QueueDetailsProvider from 'Activity/Queue/Details/QueueDetailsProvider';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import { OverflowDivider } from 'Components/Page/Toolbar/Overflow';
import PageToolbar, {
  type MoreMenuItem,
} from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import Episode from 'Episode/Episode';
import { useToggleEpisodesMonitored } from 'Episode/useEpisode';
import { Filter } from 'Filters/Filter';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { align, icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import { CheckInputChanged } from 'typings/inputs';
import { TableOptionsChangePayload } from 'typings/Table';
import getFilterValue from 'Utilities/Filter/getFilterValue';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import MissingFilterModal from './MissingFilterModal';
import {
  setMissingOption,
  setMissingOptions,
  setMissingSort,
  useMissingOptions,
} from './missingOptionsStore';
import MissingRow from './MissingRow';
import useMissing, { FILTERS, useFilters } from './useMissing';

function getMonitoredValue(
  filters: Filter[],
  selectedFilterKey: string | number
): boolean {
  return !!getFilterValue(filters, selectedFilterKey, 'monitored', false);
}

function MissingContent() {
  const executeCommand = useExecuteCommand();

  const {
    records,
    totalPages,
    totalRecords,
    error,
    isFetching,
    isLoading,
    page,
    goToPage,
    refetch,
  } = useMissing();

  const { columns, pageSize, sortKey, sortDirection, selectedFilterKey } =
    useMissingOptions();

  const filters = useFilters();
  const customFilters = useCustomFiltersList('wanted.missing');

  const isSearchingForAllEpisodes = useCommandExecuting(
    CommandNames.MissingEpisodeSearch
  );
  const isSearchingForSelectedEpisodes = useCommandExecuting(
    CommandNames.EpisodeSearch
  );

  const {
    allSelected,
    allUnselected,
    anySelected,
    getSelectedIds,
    selectAll,
    unselectAll,
  } = useSelect<Episode>();

  const [isConfirmSearchAllModalOpen, setIsConfirmSearchAllModalOpen] =
    useState(false);

  const [isInteractiveImportModalOpen, setIsInteractiveImportModalOpen] =
    useState(false);

  const { toggleEpisodesMonitored, isToggling } = useToggleEpisodesMonitored([
    '/wanted/missing',
  ]);

  const isShowingMonitored = getMonitoredValue(FILTERS, selectedFilterKey);
  const isSearchingForEpisodes =
    isSearchingForAllEpisodes || isSearchingForSelectedEpisodes;

  const episodeIds = useMemo(() => {
    return selectUniqueIds<Episode, number>(records, 'id');
  }, [records]);

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        selectAll();
      } else {
        unselectAll();
      }
    },
    [selectAll, unselectAll]
  );

  const handleSearchSelectedPress = useCallback(() => {
    executeCommand(
      {
        name: CommandNames.EpisodeSearch,
        episodeIds: getSelectedIds(),
      },
      () => {
        refetch();
      }
    );
  }, [getSelectedIds, executeCommand, refetch]);

  const handleSearchAllPress = useCallback(() => {
    setIsConfirmSearchAllModalOpen(true);
  }, []);

  const handleConfirmSearchAllMissingModalClose = useCallback(() => {
    setIsConfirmSearchAllModalOpen(false);
  }, []);

  const handleSearchAllMissingConfirmed = useCallback(() => {
    executeCommand(
      {
        name: CommandNames.MissingEpisodeSearch,
      },
      () => {
        refetch();
      }
    );

    setIsConfirmSearchAllModalOpen(false);
  }, [executeCommand, refetch]);

  const handleToggleSelectedPress = useCallback(() => {
    toggleEpisodesMonitored({
      episodeIds: getSelectedIds(),
      monitored: !isShowingMonitored,
    });
  }, [isShowingMonitored, getSelectedIds, toggleEpisodesMonitored]);

  const handleInteractiveImportPress = useCallback(() => {
    setIsInteractiveImportModalOpen(true);
  }, []);

  const handleInteractiveImportModalClose = useCallback(() => {
    setIsInteractiveImportModalOpen(false);
  }, []);

  const handleFilterSelect = useCallback((filterKey: number | string) => {
    setMissingOption('selectedFilterKey', filterKey);
  }, []);

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setMissingSort({
        sortKey,
        sortDirection,
      });
    },
    []
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      setMissingOptions(payload);

      if (payload.pageSize) {
        goToPage(1);
      }
    },
    [goToPage]
  );

  const [isTableOptionsModalOpen, setIsTableOptionsModalOpen] = useState(false);

  const onTableOptionsPress = useCallback(() => {
    setIsTableOptionsModalOpen(true);
  }, []);

  const onTableOptionsModalClose = useCallback(() => {
    setIsTableOptionsModalOpen(false);
  }, []);

  const moreMenuItems = useMemo<MoreMenuItem[]>(
    () => [
      {
        id: 'search',
        label: anySelected
          ? translate('SearchSelected')
          : translate('SearchAll'),
        iconName: icons.SEARCH,
        isDisabled: isSearchingForEpisodes,
        isSpinning: isSearchingForEpisodes,
        onPress: anySelected ? handleSearchSelectedPress : handleSearchAllPress,
      },
      {
        id: 'toggle-monitored',
        label: isShowingMonitored
          ? translate('UnmonitorSelected')
          : translate('MonitorSelected'),
        iconName: icons.MONITORED,
        isDisabled: !anySelected,
        isSpinning: isToggling,
        onPress: handleToggleSelectedPress,
      },
      {
        id: 'manual-import',
        label: translate('ManualImport'),
        iconName: icons.INTERACTIVE,
        onPress: handleInteractiveImportPress,
      },
      {
        id: 'options',
        label: translate('Options'),
        iconName: icons.TABLE,
        onPress: onTableOptionsPress,
      },
    ],
    [
      anySelected,
      isSearchingForEpisodes,
      handleSearchSelectedPress,
      handleSearchAllPress,
      isShowingMonitored,
      isToggling,
      handleToggleSelectedPress,
      handleInteractiveImportPress,
      onTableOptionsPress,
    ]
  );

  useEffect(() => {
    const repopulate = () => {
      refetch();
    };

    registerPagePopulator(repopulate, [
      'seriesUpdated',
      'episodeFileUpdated',
      'episodeFileDeleted',
    ]);

    return () => {
      unregisterPagePopulator(repopulate);
    };
  }, [refetch]);

  return (
    <QueueDetailsProvider episodeIds={episodeIds}>
      <PageContent title={translate('Missing')}>
        <PageToolbar moreMenuItems={moreMenuItems}>
          <ToolbarItem id="search" priority={1} groupId="left-a">
            <PageToolbarButton
              label={
                anySelected
                  ? translate('SearchSelected')
                  : translate('SearchAll')
              }
              iconName={icons.SEARCH}
              isDisabled={isSearchingForEpisodes}
              isSpinning={isSearchingForEpisodes}
              onPress={
                anySelected ? handleSearchSelectedPress : handleSearchAllPress
              }
            />
          </ToolbarItem>

          <OverflowDivider groupId="left-a">
            <PageToolbarSeparator />
          </OverflowDivider>

          <ToolbarItem id="toggle-monitored" priority={1} groupId="left-b">
            <PageToolbarButton
              label={
                isShowingMonitored
                  ? translate('UnmonitorSelected')
                  : translate('MonitorSelected')
              }
              iconName={icons.MONITORED}
              isDisabled={!anySelected}
              isSpinning={isToggling}
              onPress={handleToggleSelectedPress}
            />
          </ToolbarItem>

          <OverflowDivider groupId="left-b">
            <PageToolbarSeparator />
          </OverflowDivider>

          <ToolbarItem id="manual-import" priority={1} groupId="left-c">
            <PageToolbarButton
              label={translate('ManualImport')}
              iconName={icons.INTERACTIVE}
              onPress={handleInteractiveImportPress}
            />
          </ToolbarItem>

          <PageToolbarSpacer />

          <ToolbarItem id="options" priority={2} groupId="right-a">
            <TableOptionsModalWrapper
              columns={columns}
              pageSize={pageSize}
              isOpen={isTableOptionsModalOpen}
              onPress={onTableOptionsPress}
              onModalClose={onTableOptionsModalClose}
              onTableOptionChange={handleTableOptionChange}
            >
              <PageToolbarButton
                label={translate('Options')}
                iconName={icons.TABLE}
              />
            </TableOptionsModalWrapper>
          </ToolbarItem>

          <ToolbarItem id="filter" pinned={true}>
            <FilterMenu
              alignMenu={align.RIGHT}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              filterModalConnectorComponent={MissingFilterModal}
              onFilterSelect={handleFilterSelect}
            />
          </ToolbarItem>
        </PageToolbar>

        <PageContentBody>
          {isFetching && isLoading ? <LoadingIndicator /> : null}

          {!isFetching && error ? (
            <Alert kind={kinds.DANGER}>{translate('MissingLoadError')}</Alert>
          ) : null}

          {!isLoading && !error && !records.length ? (
            <Alert kind={kinds.INFO}>{translate('MissingNoItems')}</Alert>
          ) : null}

          {!isLoading && !error && !!records.length ? (
            <div>
              <Table
                selectAll={true}
                allSelected={allSelected}
                allUnselected={allUnselected}
                columns={columns}
                pageSize={pageSize}
                sortKey={sortKey}
                sortDirection={sortDirection}
                onTableOptionChange={handleTableOptionChange}
                onSelectAllChange={handleSelectAllChange}
                onSortPress={handleSortPress}
              >
                <TableBody>
                  {records.map((item) => {
                    return (
                      <MissingRow key={item.id} columns={columns} {...item} />
                    );
                  })}
                </TableBody>
              </Table>

              <TablePager
                page={page}
                totalPages={totalPages}
                totalRecords={totalRecords}
                isFetching={isFetching}
                onPageSelect={goToPage}
              />

              <ConfirmModal
                isOpen={isConfirmSearchAllModalOpen}
                kind={kinds.DANGER}
                title={translate('SearchForAllMissingEpisodes')}
                message={
                  <div>
                    <div>
                      {translate(
                        'SearchForAllMissingEpisodesConfirmationCount',
                        {
                          totalRecords,
                        }
                      )}
                    </div>
                    <div>{translate('MassSearchCancelWarning')}</div>
                  </div>
                }
                confirmLabel={translate('Search')}
                onConfirm={handleSearchAllMissingConfirmed}
                onCancel={handleConfirmSearchAllMissingModalClose}
              />
            </div>
          ) : null}
        </PageContentBody>

        <InteractiveImportModal
          isOpen={isInteractiveImportModalOpen}
          onModalClose={handleInteractiveImportModalClose}
        />
      </PageContent>
    </QueueDetailsProvider>
  );
}

function Missing() {
  const { records } = useMissing();

  return (
    <SelectProvider<Episode> items={records}>
      <MissingContent />
    </SelectProvider>
  );
}

export default Missing;
