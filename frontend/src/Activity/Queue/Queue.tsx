import React, {
  ReactElement,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
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
import usePaging from 'Components/Table/usePaging';
import createEpisodesFetchingSelector from 'Episode/createEpisodesFetchingSelector';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { align, icons, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import { clearEpisodes, fetchEpisodes } from 'Store/Actions/episodeActions';
import {
  clearQueue,
  fetchQueue,
  gotoQueuePage,
  grabQueueItems,
  removeQueueItems,
  setQueueFilter,
  setQueueSort,
  setQueueTableOption,
} from 'Store/Actions/queueActions';
import { createCustomFiltersSelector } from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import QueueItem from 'typings/Queue';
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
import QueueRow from './QueueRow';
import RemoveQueueItemModal, { RemovePressProps } from './RemoveQueueItemModal';
import createQueueStatusSelector from './Status/createQueueStatusSelector';

function Queue() {
  const requestCurrentPage = useCurrentPage();
  const dispatch = useDispatch();

  const {
    isFetching,
    isPopulated,
    error,
    items,
    columns,
    selectedFilterKey,
    filters,
    sortKey,
    sortDirection,
    page,
    totalPages,
    totalRecords,
    isGrabbing,
    isRemoving,
  } = useSelector((state: AppState) => state.queue.paged);

  const { count } = useSelector(createQueueStatusSelector());
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
    return items.some((item) => {
      return selectedIds.indexOf(item.id) > -1 && item.status === 'delay';
    });
  }, [items, selectedIds]);

  const [isConfirmRemoveModalOpen, setIsConfirmRemoveModalOpen] =
    useState(false);

  const isRefreshing =
    isFetching || isEpisodesFetching || isRefreshMonitoredDownloadsExecuting;
  const isAllPopulated =
    isPopulated &&
    (isEpisodesPopulated || !items.length || items.every((e) => !e.episodeId));
  const hasError = error || episodesError;
  const selectedCount = selectedIds.length;
  const disableSelectedActions = selectedCount === 0;

  const handleSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setSelectState({ type: value ? 'selectAll' : 'unselectAll', items });
    },
    [items, setSelectState]
  );

  const handleSelectedChange = useCallback(
    ({ id, value, shiftKey = false }: SelectStateInputProps) => {
      setSelectState({
        type: 'toggleSelected',
        items,
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [items, setSelectState]
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
    dispatch(grabQueueItems({ ids: selectedIds }));
  }, [selectedIds, dispatch]);

  const handleRemoveSelectedPress = useCallback(() => {
    shouldBlockRefresh.current = true;
    setIsConfirmRemoveModalOpen(true);
  }, [setIsConfirmRemoveModalOpen]);

  const handleRemoveSelectedConfirmed = useCallback(
    (payload: RemovePressProps) => {
      shouldBlockRefresh.current = false;
      dispatch(removeQueueItems({ ids: selectedIds, ...payload }));
      setIsConfirmRemoveModalOpen(false);
    },
    [selectedIds, setIsConfirmRemoveModalOpen, dispatch]
  );

  const handleConfirmRemoveModalClose = useCallback(() => {
    shouldBlockRefresh.current = false;
    setIsConfirmRemoveModalOpen(false);
  }, [setIsConfirmRemoveModalOpen]);

  const {
    handleFirstPagePress,
    handlePreviousPagePress,
    handleNextPagePress,
    handleLastPagePress,
    handlePageSelect,
  } = usePaging({
    page,
    totalPages,
    gotoPage: gotoQueuePage,
  });

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string) => {
      dispatch(setQueueFilter({ selectedFilterKey }));
    },
    [dispatch]
  );

  const handleSortPress = useCallback(
    (sortKey: string) => {
      dispatch(setQueueSort({ sortKey }));
    },
    [dispatch]
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      dispatch(setQueueTableOption(payload));

      if (payload.pageSize) {
        dispatch(gotoQueuePage({ page: 1 }));
      }
    },
    [dispatch]
  );

  useEffect(() => {
    if (requestCurrentPage) {
      dispatch(fetchQueue());
    } else {
      dispatch(gotoQueuePage({ page: 1 }));
    }

    return () => {
      dispatch(clearQueue());
    };
  }, [requestCurrentPage, dispatch]);

  useEffect(() => {
    const episodeIds = selectUniqueIds<QueueItem, number | undefined>(
      items,
      'episodeId'
    );

    if (episodeIds.length) {
      dispatch(fetchEpisodes({ episodeIds }));
    } else {
      dispatch(clearEpisodes());
    }
  }, [items, dispatch]);

  useEffect(() => {
    const repopulate = () => {
      dispatch(fetchQueue());
    };

    registerPagePopulator(repopulate);

    return () => {
      unregisterPagePopulator(repopulate);
    };
  }, [dispatch]);

  if (!shouldBlockRefresh.current) {
    currentQueue.current = (
      <PageContentBody>
        {isRefreshing && !isAllPopulated ? <LoadingIndicator /> : null}

        {!isRefreshing && hasError ? (
          <Alert kind={kinds.DANGER}>{translate('QueueLoadError')}</Alert>
        ) : null}

        {isAllPopulated && !hasError && !items.length ? (
          <Alert kind={kinds.INFO}>
            {selectedFilterKey !== 'all' && count > 0
              ? translate('QueueFilterHasNoItems')
              : translate('QueueIsEmpty')}
          </Alert>
        ) : null}

        {isAllPopulated && !hasError && !!items.length ? (
          <div>
            <Table
              selectAll={true}
              allSelected={allSelected}
              allUnselected={allUnselected}
              columns={columns}
              sortKey={sortKey}
              sortDirection={sortDirection}
              onTableOptionChange={handleTableOptionChange}
              onSelectAllChange={handleSelectAllChange}
              onSortPress={handleSortPress}
            >
              <TableBody>
                {items.map((item) => {
                  return (
                    <QueueRow
                      key={item.id}
                      episodeId={item.episodeId}
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
              onFirstPagePress={handleFirstPagePress}
              onPreviousPagePress={handlePreviousPagePress}
              onNextPagePress={handleNextPagePress}
              onLastPagePress={handleLastPagePress}
              onPageSelect={handlePageSelect}
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
            const item = items.find((i) => i.id === id);

            return !!(item && item.downloadClientHasPostImportCategory);
          })
        }
        canIgnore={
          isConfirmRemoveModalOpen &&
          selectedIds.every((id) => {
            const item = items.find((i) => i.id === id);

            return !!(item && item.seriesId && item.episodeId);
          })
        }
        isPending={
          isConfirmRemoveModalOpen &&
          selectedIds.every((id) => {
            const item = items.find((i) => i.id === id);

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
