import React, {
  PropsWithChildren,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react';
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
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import Episode from 'Episode/Episode';
import { useToggleEpisodesMonitored } from 'Episode/useEpisode';
import EpisodeFileProvider from 'EpisodeFile/EpisodeFileProvider';
import { Filter } from 'Filters/Filter';
import { align, icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { CheckInputChanged } from 'typings/inputs';
import { TableOptionsChangePayload } from 'typings/Table';
import getFilterValue from 'Utilities/Filter/getFilterValue';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import {
  setCutoffUnmetOption,
  setCutoffUnmetOptions,
  setCutoffUnmetSort,
  useCutoffUnmetOptions,
} from './cutoffUnmetOptionsStore';
import CutoffUnmetRow from './CutoffUnmetRow';
import useCutoffUnmet, { FILTERS } from './useCutoffUnmet';

function getMonitoredValue(
  filters: Filter[],
  selectedFilterKey: string | number
): boolean {
  return !!getFilterValue(filters, selectedFilterKey, 'monitored', false);
}

function CutoffUnmetContent() {
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
  } = useCutoffUnmet();

  const { columns, pageSize, sortKey, sortDirection, selectedFilterKey } =
    useCutoffUnmetOptions();

  const isSearchingForAllEpisodes = useCommandExecuting(
    CommandNames.CutoffUnmetEpisodeSearch
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

  const { toggleEpisodesMonitored, isToggling } = useToggleEpisodesMonitored([
    '/wanted/cutoff',
  ]);

  const isShowingMonitored = getMonitoredValue(FILTERS, selectedFilterKey);
  const isSearchingForEpisodes =
    isSearchingForAllEpisodes || isSearchingForSelectedEpisodes;

  const episodeIds = useMemo(() => {
    return selectUniqueIds<Episode, number>(records, 'id');
  }, [records]);

  const episodeFileIds = useMemo(() => {
    return selectUniqueIds<Episode, number>(records, 'episodeFileId');
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

  const handleConfirmSearchAllCutoffUnmetModalClose = useCallback(() => {
    setIsConfirmSearchAllModalOpen(false);
  }, []);

  const handleSearchAllCutoffUnmetConfirmed = useCallback(() => {
    executeCommand(
      {
        name: CommandNames.CutoffUnmetEpisodeSearch,
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

  const handleFilterSelect = useCallback((filterKey: number | string) => {
    setCutoffUnmetOption('selectedFilterKey', filterKey);
  }, []);

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setCutoffUnmetSort({
        sortKey,
        sortDirection,
      });
    },
    []
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      setCutoffUnmetOptions(payload);

      if (payload.pageSize) {
        goToPage(1);
      }
    },
    [goToPage]
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
    <CutoffUnmetProvider
      episodeIds={episodeIds}
      episodeFileIds={episodeFileIds}
    >
      <PageContent title={translate('CutoffUnmet')}>
        <PageToolbar>
          <PageToolbarSection>
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

            <PageToolbarSeparator />

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
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <TableOptionsModalWrapper
              columns={columns}
              pageSize={pageSize}
              onTableOptionChange={handleTableOptionChange}
            >
              <PageToolbarButton
                label={translate('Options')}
                iconName={icons.TABLE}
              />
            </TableOptionsModalWrapper>

            <FilterMenu
              alignMenu={align.RIGHT}
              selectedFilterKey={selectedFilterKey}
              filters={FILTERS}
              customFilters={[]}
              onFilterSelect={handleFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody>
          {isFetching && isLoading ? <LoadingIndicator /> : null}

          {!isFetching && error ? (
            <Alert kind={kinds.DANGER}>
              {translate('CutoffUnmetLoadError')}
            </Alert>
          ) : null}

          {!isLoading && !error && !records.length ? (
            <Alert kind={kinds.INFO}>{translate('CutoffUnmetNoItems')}</Alert>
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
                      <CutoffUnmetRow
                        key={item.id}
                        columns={columns}
                        {...item}
                      />
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
                title={translate('SearchForCutoffUnmetEpisodes')}
                message={
                  <div>
                    <div>
                      {translate(
                        'SearchForCutoffUnmetEpisodesConfirmationCount',
                        { totalRecords }
                      )}
                    </div>
                    <div>{translate('MassSearchCancelWarning')}</div>
                  </div>
                }
                confirmLabel={translate('Search')}
                onConfirm={handleSearchAllCutoffUnmetConfirmed}
                onCancel={handleConfirmSearchAllCutoffUnmetModalClose}
              />
            </div>
          ) : null}
        </PageContentBody>
      </PageContent>
    </CutoffUnmetProvider>
  );
}

export default function CutoffUnmet() {
  const { records } = useCutoffUnmet();

  return (
    <SelectProvider<Episode> items={records}>
      <CutoffUnmetContent />
    </SelectProvider>
  );
}

function CutoffUnmetProvider({
  episodeIds,
  episodeFileIds,
  children,
}: PropsWithChildren<{ episodeIds: number[]; episodeFileIds: number[] }>) {
  return (
    <QueueDetailsProvider episodeIds={episodeIds}>
      <EpisodeFileProvider episodeFileIds={episodeFileIds}>
        {children}
      </EpisodeFileProvider>
    </QueueDetailsProvider>
  );
}
