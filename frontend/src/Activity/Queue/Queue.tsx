import React, {
  ReactElement,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
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
import useEpisodes from 'Episode/useEpisodes';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { align, icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import { CheckInputChanged } from 'typings/inputs';
import QueueModel from 'typings/Queue';
import { TableOptionsChangePayload } from 'typings/Table';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import QueueFilterModal from './QueueFilterModal';
import {
  setQueueOption,
  setQueueOptions,
  setQueueSort,
  useQueueOptions,
} from './queueOptionsStore';
import QueueRow from './QueueRow';
import RemoveQueueItemModal from './RemoveQueueItemModal';
import useQueueStatus from './Status/useQueueStatus';
import useQueue, {
  useFilters,
  useGrabQueueItems,
  useRemoveQueueItems,
} from './useQueue';

function QueueContent() {
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
  } = useQueue();

  const { columns, pageSize, sortKey, sortDirection, selectedFilterKey } =
    useQueueOptions();

  const filters = useFilters();

  const { isRemoving, removeQueueItems } = useRemoveQueueItems();
  const { isGrabbing, grabQueueItems } = useGrabQueueItems();

  const { count } = useQueueStatus();

  const episodeIds = useMemo(() => {
    return selectUniqueIds<QueueModel, number>(records, 'episodeIds');
  }, [records]);

  const {
    isFetching: isEpisodesFetching,
    isFetched: isEpisodesFetched,
    error: episodesError,
  } = useEpisodes({ episodeIds });

  const customFilters = useCustomFiltersList('queue');

  const isRefreshMonitoredDownloadsExecuting = useCommandExecuting(
    CommandNames.RefreshMonitoredDownloads
  );

  const shouldBlockRefresh = useRef(false);
  const currentQueue = useRef<ReactElement | null>(null);

  const { allSelected, allUnselected, selectAll, unselectAll, useSelectedIds } =
    useSelect<QueueModel>();

  const selectedIds = useSelectedIds();
  const isPendingSelected = useMemo(() => {
    return records.some((item) => {
      return selectedIds.indexOf(item.id) > -1 && item.status === 'delay';
    });
  }, [records, selectedIds]);

  const [isConfirmRemoveModalOpen, setIsConfirmRemoveModalOpen] =
    useState(false);

  const [isInteractiveImportDownloadIds, setIsInteractiveImportDownloadIds] =
    useState<string[]>(() => []);

  const isRefreshing =
    isLoading || isEpisodesFetching || isRefreshMonitoredDownloadsExecuting;

  // Use isLoading over isFetched to avoid losing the table UI when switching pages
  const isAllPopulated =
    !isLoading &&
    (isEpisodesFetched ||
      !records.length ||
      records.every((e) => !e.episodeIds?.length));
  const hasError = error || episodesError;
  const selectedCount = selectedIds.length;
  const disableSelectedActions = selectedCount === 0;

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

  const handleRefreshPress = useCallback(() => {
    executeCommand({
      name: CommandNames.RefreshMonitoredDownloads,
    });
  }, [executeCommand]);

  const handleQueueRowModalOpenOrClose = useCallback((isOpen: boolean) => {
    shouldBlockRefresh.current = isOpen;
  }, []);

  const handleGrabSelectedPress = useCallback(() => {
    grabQueueItems({ ids: selectedIds });
  }, [selectedIds, grabQueueItems]);

  const handleRemoveSelectedPress = useCallback(() => {
    shouldBlockRefresh.current = true;
    setIsConfirmRemoveModalOpen(true);
  }, [setIsConfirmRemoveModalOpen]);

  const handleRemoveSelectedConfirmed = useCallback(() => {
    shouldBlockRefresh.current = false;
    removeQueueItems({ ids: selectedIds });
    setIsConfirmRemoveModalOpen(false);
  }, [selectedIds, removeQueueItems]);

  const handleConfirmRemoveModalClose = useCallback(() => {
    shouldBlockRefresh.current = false;
    setIsConfirmRemoveModalOpen(false);
  }, []);

  const handleImportSelectedPress = useCallback(() => {
    shouldBlockRefresh.current = true;
    setIsInteractiveImportDownloadIds(
      selectedIds
        .map((id) => {
          const item = records.find((i) => i.id === id);

          return item?.downloadId;
        })
        .filter((id): id is string => !!id)
    );
  }, [records, selectedIds]);

  const handleImportSelectedModalClose = useCallback(() => {
    shouldBlockRefresh.current = false;
    setIsInteractiveImportDownloadIds([]);
  }, []);

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      setQueueOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setQueueSort({
        sortKey,
        sortDirection,
      });
    },
    []
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      setQueueOptions(payload);

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

    registerPagePopulator(repopulate);

    return () => {
      unregisterPagePopulator(repopulate);
    };
  }, [refetch]);

  if (!shouldBlockRefresh.current) {
    currentQueue.current = (
      <PageContentBody>
        {isRefreshing && !isAllPopulated ? <LoadingIndicator /> : null}

        {!isRefreshing && hasError ? (
          <Alert kind={kinds.DANGER}>{translate('QueueLoadError')}</Alert>
        ) : null}

        {isAllPopulated && !hasError && !records.length ? (
          <Alert kind={kinds.INFO}>
            {selectedFilterKey !== 'all' && count > 0
              ? translate('QueueFilterHasNoItems')
              : translate('QueueIsEmpty')}
          </Alert>
        ) : null}

        {isAllPopulated && !hasError && !!records.length ? (
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
                    <QueueRow
                      key={item.id}
                      columns={columns}
                      {...item}
                      onQueueRowModalOpenOrClose={
                        handleQueueRowModalOpenOrClose
                      }
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
          </div>
        ) : null}
      </PageContentBody>
    );
  }

  return (
    <PageContent title={translate('Queue')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label="Refresh"
            iconName={icons.REFRESH}
            isSpinning={isRefreshing}
            onPress={handleRefreshPress}
          />

          <PageToolbarSeparator />

          <PageToolbarButton
            label={translate('GrabSelected')}
            iconName={icons.DOWNLOAD}
            isDisabled={disableSelectedActions || !isPendingSelected}
            isSpinning={isGrabbing}
            onPress={handleGrabSelectedPress}
          />

          <PageToolbarButton
            label={translate('RemoveSelected')}
            iconName={icons.REMOVE}
            isDisabled={disableSelectedActions}
            isSpinning={isRemoving}
            onPress={handleRemoveSelectedPress}
          />

          <PageToolbarSeparator />

          <PageToolbarButton
            label={translate('ImportSelected')}
            iconName={icons.INTERACTIVE}
            isDisabled={disableSelectedActions}
            onPress={handleImportSelectedPress}
          />
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT}>
          <TableOptionsModalWrapper
            columns={columns}
            pageSize={pageSize}
            maxPageSize={200}
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
            filters={filters}
            customFilters={customFilters}
            filterModalConnectorComponent={QueueFilterModal}
            onFilterSelect={handleFilterSelect}
          />
        </PageToolbarSection>
      </PageToolbar>

      {currentQueue.current}

      <RemoveQueueItemModal
        isOpen={isConfirmRemoveModalOpen}
        selectedCount={selectedCount}
        canChangeCategory={
          isConfirmRemoveModalOpen &&
          selectedIds.every((id: number) => {
            const item = records.find((i) => i.id === id);

            return !!(item && item.downloadClientHasPostImportCategory);
          })
        }
        canIgnore={
          isConfirmRemoveModalOpen &&
          selectedIds.every((id: number) => {
            const item = records.find((i) => i.id === id);

            return !!(item && item.seriesId && item.episodeId);
          })
        }
        isPending={
          isConfirmRemoveModalOpen &&
          selectedIds.every((id: number) => {
            const item = records.find((i) => i.id === id);

            if (!item) {
              return false;
            }

            return (
              item.status === 'delay' ||
              item.status === 'downloadClientUnavailable'
            );
          })
        }
        onRemovePress={handleRemoveSelectedConfirmed}
        onModalClose={handleConfirmRemoveModalClose}
      />

      <InteractiveImportModal
        isOpen={isInteractiveImportDownloadIds.length > 0}
        downloadIds={isInteractiveImportDownloadIds}
        title={translate('InteractiveImportMultipleQueueItems')}
        onModalClose={handleImportSelectedModalClose}
      />
    </PageContent>
  );
}

function Queue() {
  const { records } = useQueue();

  return (
    <SelectProvider<QueueModel> items={records}>
      <QueueContent />
    </SelectProvider>
  );
}

export default Queue;
