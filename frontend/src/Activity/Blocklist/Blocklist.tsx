import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { setQueueOptions } from 'Activity/Queue/queueOptionsStore';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
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
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { align, icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import BlockListModel from 'typings/Blocklist';
import { CheckInputChanged } from 'typings/inputs';
import { TableOptionsChangePayload } from 'typings/Table';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import BlocklistFilterModal from './BlocklistFilterModal';
import {
  setBlocklistOption,
  setBlocklistSort,
  useBlocklistOptions,
} from './blocklistOptionsStore';
import BlocklistRow from './BlocklistRow';
import useBlocklist, {
  useFilters,
  useRemoveBlocklistItems,
} from './useBlocklist';

function BlocklistContent() {
  const {
    records,
    totalPages,
    totalRecords,
    isFetching,
    isFetched,
    isLoading,
    error,
    page,
    goToPage,
    refetch,
  } = useBlocklist();

  const { columns, pageSize, sortKey, sortDirection, selectedFilterKey } =
    useBlocklistOptions();

  const filters = useFilters();
  const { isRemoving, removeBlocklistItems } = useRemoveBlocklistItems();

  const customFilters = useCustomFiltersList('blocklist');
  const isClearingBlocklistExecuting = useSelector(
    createCommandExecutingSelector(commandNames.CLEAR_BLOCKLIST)
  );
  const dispatch = useDispatch();

  const [isConfirmRemoveModalOpen, setIsConfirmRemoveModalOpen] =
    useState(false);
  const [isConfirmClearModalOpen, setIsConfirmClearModalOpen] = useState(false);

  const {
    allSelected,
    allUnselected,
    anySelected,
    getSelectedIds,
    selectAll,
    unselectAll,
  } = useSelect<BlockListModel>();

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

  const handleRemoveSelectedPress = useCallback(() => {
    setIsConfirmRemoveModalOpen(true);
  }, [setIsConfirmRemoveModalOpen]);

  const handleRemoveSelectedConfirmed = useCallback(() => {
    removeBlocklistItems({ ids: getSelectedIds() });
    setIsConfirmRemoveModalOpen(false);
  }, [getSelectedIds, setIsConfirmRemoveModalOpen, removeBlocklistItems]);

  const handleConfirmRemoveModalClose = useCallback(() => {
    setIsConfirmRemoveModalOpen(false);
  }, [setIsConfirmRemoveModalOpen]);

  const handleClearBlocklistPress = useCallback(() => {
    setIsConfirmClearModalOpen(true);
  }, [setIsConfirmClearModalOpen]);

  const handleClearBlocklistConfirmed = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.CLEAR_BLOCKLIST,
        commandFinished: () => {
          goToPage(1);
        },
      })
    );
    setIsConfirmClearModalOpen(false);
  }, [setIsConfirmClearModalOpen, goToPage, dispatch]);

  const handleConfirmClearModalClose = useCallback(() => {
    setIsConfirmClearModalOpen(false);
  }, [setIsConfirmClearModalOpen]);

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      setBlocklistOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setBlocklistSort({
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

  return (
    <PageContent title={translate('Blocklist')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('RemoveSelected')}
            iconName={icons.REMOVE}
            isDisabled={!anySelected}
            isSpinning={isRemoving}
            onPress={handleRemoveSelectedPress}
          />

          <PageToolbarButton
            label={translate('Clear')}
            iconName={icons.CLEAR}
            isDisabled={!records.length}
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
        {isLoading && !isFetched ? <LoadingIndicator /> : null}

        {!isLoading && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('BlocklistLoadError')}</Alert>
        ) : null}

        {isFetched && !error && !records.length ? (
          <Alert kind={kinds.INFO}>
            {selectedFilterKey === 'all'
              ? translate('NoBlocklistItems')
              : translate('BlocklistFilterHasNoItems')}
          </Alert>
        ) : null}

        {isFetched && !error && !!records.length ? (
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
                    <BlocklistRow key={item.id} columns={columns} {...item} />
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
  );
}

function Blocklist() {
  const { records } = useBlocklist();

  return (
    <SelectProvider<BlockListModel> items={records}>
      <BlocklistContent />
    </SelectProvider>
  );
}

export default Blocklist;
