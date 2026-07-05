import React, { useCallback, useEffect, useState } from 'react';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import PageMessage from 'Components/Page/PageMessage';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModal from 'Components/Table/TableOptions/TableOptionsModal';
import TablePager from 'Components/Table/TablePager';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { align, icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
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
  setBlocklistOptions,
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
  const executeCommand = useExecuteCommand();
  const isClearingBlocklistExecuting = useCommandExecuting(
    CommandNames.ClearBlocklist
  );

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
    executeCommand({ name: CommandNames.ClearBlocklist }, () => {
      goToPage(1);
      refetch();
    });
    setIsConfirmClearModalOpen(false);
  }, [setIsConfirmClearModalOpen, executeCommand, goToPage, refetch]);

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
      setBlocklistOptions(payload);

      if (payload.pageSize) {
        goToPage(1);
      }
    },
    [goToPage]
  );

  const [isTableOptionsModalOpen, setIsTableOptionsModalOpen] = useState(false);

  const handleTableOptionsPress = useCallback(() => {
    setIsTableOptionsModalOpen(true);
  }, []);

  const handleTableOptionsModalClose = useCallback(() => {
    setIsTableOptionsModalOpen(false);
  }, []);

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
        <ToolbarItem
          id="remove-selected"
          priority={1}
          groupId="left"
          label={translate('RemoveSelected')}
          iconName={icons.REMOVE}
          isDisabled={!anySelected}
          isSpinning={isRemoving}
          onPress={handleRemoveSelectedPress}
        />

        <ToolbarItem
          id="clear"
          priority={1}
          groupId="left"
          label={translate('Clear')}
          iconName={icons.CLEAR}
          isDisabled={!records.length}
          isSpinning={isClearingBlocklistExecuting}
          onPress={handleClearBlocklistPress}
        />

        <PageToolbarSpacer />

        <ToolbarItem
          id="options"
          priority={2}
          groupId="right"
          label={translate('Options')}
          iconName={icons.TABLE}
          onPress={handleTableOptionsPress}
        />

        <ToolbarItem id="filter" pinned={true}>
          <FilterMenu
            alignMenu={align.RIGHT}
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={customFilters}
            filterModalConnectorComponent={BlocklistFilterModal}
            onFilterSelect={handleFilterSelect}
          />
        </ToolbarItem>
      </PageToolbar>

      <PageContentBody>
        <PageHeading
          scope={translate('Activity')}
          title={translate('Blocklist')}
        />

        {isLoading && !isFetched ? <LoadingIndicator /> : null}

        {!isLoading && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('BlocklistLoadError')}</Alert>
        ) : null}

        {isFetched && !error && !records.length ? (
          <PageMessage>
            {selectedFilterKey === 'all'
              ? translate('NoBlocklistItems')
              : translate('BlocklistFilterHasNoItems')}
          </PageMessage>
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

      <TableOptionsModal
        isOpen={isTableOptionsModalOpen}
        columns={columns}
        pageSize={pageSize}
        onTableOptionChange={handleTableOptionChange}
        onModalClose={handleTableOptionsModalClose}
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
