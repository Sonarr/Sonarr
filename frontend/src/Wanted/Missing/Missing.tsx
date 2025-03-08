import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState, { Filter } from 'App/State/AppState';
import * as commandNames from 'Commands/commandNames';
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
import usePaging from 'Components/Table/usePaging';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { align, icons, kinds } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  batchToggleMissingEpisodes,
  clearMissing,
  fetchMissing,
  gotoMissingPage,
  setMissingFilter,
  setMissingSort,
  setMissingTableOption,
} from 'Store/Actions/wantedActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import { TableOptionsChangePayload } from 'typings/Table';
import getFilterValue from 'Utilities/Filter/getFilterValue';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import MissingRow from './MissingRow';

function getMonitoredValue(
  filters: Filter[],
  selectedFilterKey: string
): boolean {
  return !!getFilterValue(filters, selectedFilterKey, 'monitored', false);
}

function Missing() {
  const dispatch = useDispatch();
  const requestCurrentPage = useCurrentPage();

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
    pageSize,
    totalPages,
    totalRecords = 0,
  } = useSelector((state: AppState) => state.wanted.missing);

  const isSearchingForAllEpisodes = useSelector(
    createCommandExecutingSelector(commandNames.CUTOFF_UNMET_EPISODE_SEARCH)
  );
  const isSearchingForSelectedEpisodes = useSelector(
    createCommandExecutingSelector(commandNames.EPISODE_SEARCH)
  );

  const [selectState, setSelectState] = useSelectState();
  const { allSelected, allUnselected, selectedState } = selectState;

  const [isConfirmSearchAllModalOpen, setIsConfirmSearchAllModalOpen] =
    useState(false);

  const [isInteractiveImportModalOpen, setIsInteractiveImportModalOpen] =
    useState(false);

  const {
    handleFirstPagePress,
    handlePreviousPagePress,
    handleNextPagePress,
    handleLastPagePress,
    handlePageSelect,
  } = usePaging({
    page,
    totalPages,
    gotoPage: gotoMissingPage,
  });

  const selectedIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const isSaving = useMemo(() => {
    return items.filter((m) => m.isSaving).length > 1;
  }, [items]);

  const itemsSelected = !!selectedIds.length;
  const isShowingMonitored = getMonitoredValue(filters, selectedFilterKey);
  const isSearchingForEpisodes =
    isSearchingForAllEpisodes || isSearchingForSelectedEpisodes;

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

  const handleSearchSelectedPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.EPISODE_SEARCH,
        episodeIds: selectedIds,
        commandFinished: () => {
          dispatch(fetchMissing());
        },
      })
    );
  }, [selectedIds, dispatch]);

  const handleSearchAllPress = useCallback(() => {
    setIsConfirmSearchAllModalOpen(true);
  }, []);

  const handleConfirmSearchAllMissingModalClose = useCallback(() => {
    setIsConfirmSearchAllModalOpen(false);
  }, []);

  const handleSearchAllMissingConfirmed = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.CUTOFF_UNMET_EPISODE_SEARCH,
        commandFinished: () => {
          dispatch(fetchMissing());
        },
      })
    );

    setIsConfirmSearchAllModalOpen(false);
  }, [dispatch]);

  const handleToggleSelectedPress = useCallback(() => {
    dispatch(
      batchToggleMissingEpisodes({
        episodeIds: selectedIds,
        monitored: !isShowingMonitored,
      })
    );
  }, [isShowingMonitored, selectedIds, dispatch]);

  const handleInteractiveImportPress = useCallback(() => {
    setIsInteractiveImportModalOpen(true);
  }, []);

  const handleInteractiveImportModalClose = useCallback(() => {
    setIsInteractiveImportModalOpen(false);
  }, []);

  const handleFilterSelect = useCallback(
    (filterKey: number | string) => {
      dispatch(setMissingFilter({ selectedFilterKey: filterKey }));
    },
    [dispatch]
  );

  const handleSortPress = useCallback(
    (sortKey: string) => {
      dispatch(setMissingSort({ sortKey }));
    },
    [dispatch]
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      dispatch(setMissingTableOption(payload));

      if (payload.pageSize) {
        dispatch(gotoMissingPage({ page: 1 }));
      }
    },
    [dispatch]
  );

  useEffect(() => {
    if (requestCurrentPage) {
      dispatch(fetchMissing());
    } else {
      dispatch(gotoMissingPage({ page: 1 }));
    }

    return () => {
      dispatch(clearMissing());
    };
  }, [requestCurrentPage, dispatch]);

  useEffect(() => {
    const repopulate = () => {
      dispatch(fetchMissing());
    };

    registerPagePopulator(repopulate, [
      'seriesUpdated',
      'episodeFileUpdated',
      'episodeFileDeleted',
    ]);

    return () => {
      unregisterPagePopulator(repopulate);
    };
  }, [dispatch]);

  return (
    <PageContent title={translate('Missing')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={
              itemsSelected
                ? translate('SearchSelected')
                : translate('SearchAll')
            }
            iconName={icons.SEARCH}
            isDisabled={isSearchingForEpisodes}
            isSpinning={isSearchingForEpisodes}
            onPress={
              itemsSelected ? handleSearchSelectedPress : handleSearchAllPress
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
            isDisabled={!itemsSelected}
            isSpinning={isSaving}
            onPress={handleToggleSelectedPress}
          />

          <PageToolbarSeparator />

          <PageToolbarButton
            label={translate('ManualImport')}
            iconName={icons.INTERACTIVE}
            onPress={handleInteractiveImportPress}
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
            filters={filters}
            customFilters={[]}
            onFilterSelect={handleFilterSelect}
          />
        </PageToolbarSection>
      </PageToolbar>

      <PageContentBody>
        {isFetching && !isPopulated ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>{translate('MissingLoadError')}</Alert>
        ) : null}

        {isPopulated && !error && !items.length ? (
          <Alert kind={kinds.INFO}>{translate('MissingNoItems')}</Alert>
        ) : null}

        {isPopulated && !error && !!items.length ? (
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
                {items.map((item) => {
                  return (
                    <MissingRow
                      key={item.id}
                      isSelected={selectedState[item.id]}
                      columns={columns}
                      {...item}
                      onSelectedChange={handleSelectedChange}
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

            <ConfirmModal
              isOpen={isConfirmSearchAllModalOpen}
              kind={kinds.DANGER}
              title={translate('SearchForMissingEpisodes')}
              message={
                <div>
                  <div>
                    {translate('SearchForMissingEpisodesConfirmationCount', {
                      totalRecords,
                    })}
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
  );
}

export default Missing;
