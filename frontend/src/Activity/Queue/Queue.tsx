import React, {
  ReactElement,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import * as commandNames from 'Commands/commandNames';
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
import createEpisodesFetchingSelector from 'Episode/createEpisodesFetchingSelector';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { align, icons, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import { clearEpisodes, fetchEpisodes } from 'Store/Actions/episodeActions';
import { createCustomFiltersSelector } from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import { TableOptionsChangePayload } from 'typings/Table';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import QueueFilterModal from './QueueFilterModal';
import QueueOptions from './QueueOptions';
import {
  setQueueOption,
  setQueueOptions,
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

function Queue() {
  const dispatch = useDispatch();

  const {
    records,
    totalPages,
    totalRecords,
    error,
    isFetching,
    isFetched,
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
  const { isEpisodesFetching, isEpisodesPopulated, episodesError } =
    useSelector(createEpisodesFetchingSelector());
  const customFilters = useSelector(createCustomFiltersSelector('queue'));

  const isRefreshMonitoredDownloadsExecuting = useSelector(
    createCommandExecutingSelector(commandNames.REFRESH_MONITORED_DOWNLOADS)
  );

  const shouldBlockRefresh = useRef(false);
  const currentQueue = useRef<ReactElement | null>(null);

  const [selectState, setSelectState] = useSelectState();
  const { allSelected, allUnselected, selectedState } = selectState;

  const selectedIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const isPendingSelected = useMemo(() => {
    return records.some((item) => {
      return selectedIds.indexOf(item.id) > -1 && item.status === 'delay';
    });
  }, [records, selectedIds]);

  const [isConfirmRemoveModalOpen, setIsConfirmRemoveModalOpen] =
    useState(false);

  const isRefreshing =
    isLoading || isEpisodesFetching || isRefreshMonitoredDownloadsExecuting;
  const isAllPopulated =
    isFetched &&
    (isEpisodesPopulated ||
      !records.length ||
      records.every((e) => !e.episodeIds?.length));
  const hasError = error || episodesError;
  const selectedCount = selectedIds.length;
  const disableSelectedActions = selectedCount === 0;

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setSelectState({
        type: value ? 'selectAll' : 'unselectAll',
        items: records,
      });
    },
    [records, setSelectState]
  );

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey = false }: SelectStateInputProps) => {
      setSelectState({
        type: 'toggleSelected',
        items: records,
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [records, setSelectState]
  );

  const handleRefreshPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.REFRESH_MONITORED_DOWNLOADS,
      })
    );
  }, [dispatch]);

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
  }, [selectedIds, setIsConfirmRemoveModalOpen, removeQueueItems]);

  const handleConfirmRemoveModalClose = useCallback(() => {
    shouldBlockRefresh.current = false;
    setIsConfirmRemoveModalOpen(false);
  }, [setIsConfirmRemoveModalOpen]);

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      setQueueOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  const handleSortPress = useCallback((sortKey: string) => {
    setQueueOption('sortKey', sortKey);
  }, []);

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
    const episodeIds = selectUniqueIds(records, 'episodeIds');

    if (episodeIds.length) {
      dispatch(fetchEpisodes({ episodeIds }));
    } else {
      dispatch(clearEpisodes());
    }
  }, [records, dispatch]);

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
              optionsComponent={QueueOptions}
              onTableOptionChange={handleTableOptionChange}
              onSelectAllChange={handleSelectAllChange}
              onSortPress={handleSortPress}
            >
              <TableBody>
                {records.map((item) => {
                  return (
                    <QueueRow
                      key={item.id}
                      isSelected={selectedState[item.id]}
                      columns={columns}
                      {...item}
                      onSelectedChange={handleSelectedChange}
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
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT}>
          <TableOptionsModalWrapper
            columns={columns}
            pageSize={pageSize}
            maxPageSize={200}
            optionsComponent={QueueOptions}
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
          selectedIds.every((id) => {
            const item = records.find((i) => i.id === id);

            return !!(item && item.downloadClientHasPostImportCategory);
          })
        }
        canIgnore={
          isConfirmRemoveModalOpen &&
          selectedIds.every((id) => {
            const item = records.find((i) => i.id === id);

            return !!(item && item.seriesId && item.episodeId);
          })
        }
        isPending={
          isConfirmRemoveModalOpen &&
          selectedIds.every((id) => {
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
    </PageContent>
  );
}

export default Queue;
