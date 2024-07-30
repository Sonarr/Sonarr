import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { SelectProvider } from 'App/SelectContext';
import AppState from 'App/State/AppState';
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
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import usePaging from 'Components/Table/usePaging';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { align, icons, kinds } from 'Helpers/Props';
import {
  clearBlocklist,
  fetchBlocklist,
  gotoBlocklistPage,
  removeBlocklistItems,
  setBlocklistFilter,
  setBlocklistSort,
  setBlocklistTableOption,
} from 'Store/Actions/blocklistActions';
import { executeCommand } from 'Store/Actions/commandActions';
import { createCustomFiltersSelector } from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import { TableOptionsChangePayload } from 'typings/Table';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import BlocklistFilterModal from './BlocklistFilterModal';
import BlocklistRow from './BlocklistRow';

function Blocklist() {
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
    totalRecords,
    isRemoving,
  } = useSelector((state: AppState) => state.blocklist);

  const customFilters = useSelector(createCustomFiltersSelector('blocklist'));
  const isClearingBlocklistExecuting = useSelector(
    createCommandExecutingSelector(commandNames.CLEAR_BLOCKLIST)
  );
  const dispatch = useDispatch();

  const [isConfirmRemoveModalOpen, setIsConfirmRemoveModalOpen] =
    useState(false);
  const [isConfirmClearModalOpen, setIsConfirmClearModalOpen] = useState(false);

  const [selectState, setSelectState] = useSelectState();
  const { allSelected, allUnselected, selectedState } = selectState;

  const selectedIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const wasClearingBlocklistExecuting = usePrevious(
    isClearingBlocklistExecuting
  );

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

  const handleRemoveSelectedPress = useCallback(() => {
    setIsConfirmRemoveModalOpen(true);
  }, [setIsConfirmRemoveModalOpen]);

  const handleRemoveSelectedConfirmed = useCallback(() => {
    dispatch(removeBlocklistItems({ ids: selectedIds }));
    setIsConfirmRemoveModalOpen(false);
  }, [selectedIds, setIsConfirmRemoveModalOpen, dispatch]);

  const handleConfirmRemoveModalClose = useCallback(() => {
    setIsConfirmRemoveModalOpen(false);
  }, [setIsConfirmRemoveModalOpen]);

  const handleClearBlocklistPress = useCallback(() => {
    setIsConfirmClearModalOpen(true);
  }, [setIsConfirmClearModalOpen]);

  const handleClearBlocklistConfirmed = useCallback(() => {
    dispatch(executeCommand({ name: commandNames.CLEAR_BLOCKLIST }));
    setIsConfirmClearModalOpen(false);
  }, [setIsConfirmClearModalOpen, dispatch]);

  const handleConfirmClearModalClose = useCallback(() => {
    setIsConfirmClearModalOpen(false);
  }, [setIsConfirmClearModalOpen]);

  const {
    handleFirstPagePress,
    handlePreviousPagePress,
    handleNextPagePress,
    handleLastPagePress,
    handlePageSelect,
  } = usePaging({
    page,
    totalPages,
    gotoPage: gotoBlocklistPage,
  });

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string) => {
      dispatch(setBlocklistFilter({ selectedFilterKey }));
    },
    [dispatch]
  );

  const handleSortPress = useCallback(
    (sortKey: string) => {
      dispatch(setBlocklistSort({ sortKey }));
    },
    [dispatch]
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      dispatch(setBlocklistTableOption(payload));

      if (payload.pageSize) {
        dispatch(gotoBlocklistPage({ page: 1 }));
      }
    },
    [dispatch]
  );

  useEffect(() => {
    if (requestCurrentPage) {
      dispatch(fetchBlocklist());
    } else {
      dispatch(gotoBlocklistPage({ page: 1 }));
    }

    return () => {
      dispatch(clearBlocklist());
    };
  }, [requestCurrentPage, dispatch]);

  useEffect(() => {
    const repopulate = () => {
      dispatch(fetchBlocklist());
    };

    registerPagePopulator(repopulate);

    return () => {
      unregisterPagePopulator(repopulate);
    };
  }, [dispatch]);

  useEffect(() => {
    if (wasClearingBlocklistExecuting && !isClearingBlocklistExecuting) {
      dispatch(gotoBlocklistPage({ page: 1 }));
    }
  }, [isClearingBlocklistExecuting, wasClearingBlocklistExecuting, dispatch]);

  return (
    <SelectProvider items={items}>
      <PageContent title={translate('Blocklist')}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RemoveSelected')}
              iconName={icons.REMOVE}
              isDisabled={!selectedIds.length}
              isSpinning={isRemoving}
              onPress={handleRemoveSelectedPress}
            />

            <PageToolbarButton
              label={translate('Clear')}
              iconName={icons.CLEAR}
              isDisabled={!items.length}
              isSpinning={isClearingBlocklistExecuting}
              onPress={handleClearBlocklistPress}
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
              customFilters={customFilters}
              filterModalConnectorComponent={BlocklistFilterModal}
              onFilterSelect={handleFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody>
          {isFetching && !isPopulated ? <LoadingIndicator /> : null}

          {!isFetching && !!error ? (
            <Alert kind={kinds.DANGER}>{translate('BlocklistLoadError')}</Alert>
          ) : null}

          {isPopulated && !error && !items.length ? (
            <Alert kind={kinds.INFO}>
              {selectedFilterKey === 'all'
                ? translate('NoBlocklistItems')
                : translate('BlocklistFilterHasNoItems')}
            </Alert>
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
                      <BlocklistRow
                        key={item.id}
                        isSelected={selectedState[item.id] || false}
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
            </div>
          ) : null}
        </PageContentBody>

        <ConfirmModal
          isOpen={isConfirmRemoveModalOpen}
          kind={kinds.DANGER}
          title={translate('RemoveSelected')}
          message={translate('RemoveSelectedBlocklistMessageText')}
          confirmLabel={translate('RemoveSelected')}
          onConfirm={handleRemoveSelectedConfirmed}
          onCancel={handleConfirmRemoveModalClose}
        />

        <ConfirmModal
          isOpen={isConfirmClearModalOpen}
          kind={kinds.DANGER}
          title={translate('ClearBlocklist')}
          message={translate('ClearBlocklistMessageText')}
          confirmLabel={translate('Clear')}
          onConfirm={handleClearBlocklistConfirmed}
          onCancel={handleConfirmClearModalClose}
        />
      </PageContent>
    </SelectProvider>
  );
}

export default Blocklist;
