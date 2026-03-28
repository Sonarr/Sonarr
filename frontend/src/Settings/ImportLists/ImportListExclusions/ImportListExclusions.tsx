import React, { useCallback, useEffect, useState } from 'react';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import FieldSet from 'Components/FieldSet';
import IconButton from 'Components/Link/IconButton';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import PageSectionContent from 'Components/Page/PageSectionContent';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TablePager from 'Components/Table/TablePager';
import TableRow from 'Components/Table/TableRow';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { CheckInputChanged } from 'typings/inputs';
import { TableOptionsChangePayload } from 'typings/Table';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModal from './EditImportListExclusionModal';
import {
  setImportListExclusionOption,
  setImportListExclusionSort,
  useImportListExclusionOptions,
} from './importListExclusionOptionsStore';
import ImportListExclusionRow from './ImportListExclusionRow';
import useImportListExclusions, {
  ImportListExclusion,
  useDeleteImportListExclusions,
} from './useImportListExclusions';
import styles from './ImportListExclusions.css';

const COLUMNS: Column[] = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
    isSortable: true,
  },
  {
    name: 'tvdbId',
    label: () => translate('TvdbId'),
    isVisible: true,
    isSortable: true,
  },
  {
    className: styles.actions,
    name: 'actions',
    label: '',
    isVisible: true,
    isSortable: false,
  },
];

function ImportListExclusionsContent() {
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
  } = useImportListExclusions();

  const { pageSize, sortKey, sortDirection } = useImportListExclusionOptions();

  const { deleteImportListExclusions, isDeleting } =
    useDeleteImportListExclusions();

  const [isConfirmDeleteModalOpen, setIsConfirmDeleteModalOpen] =
    useState(false);

  const {
    allSelected,
    allUnselected,
    anySelected,
    getSelectedIds,
    selectAll,
    unselectAll,
  } = useSelect<ImportListExclusion>();

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

  const handleDeleteSelectedPress = useCallback(() => {
    setIsConfirmDeleteModalOpen(true);
  }, []);

  const handleDeleteSelectedConfirmed = useCallback(() => {
    deleteImportListExclusions({ ids: getSelectedIds() });
    setIsConfirmDeleteModalOpen(false);
    unselectAll();
  }, [getSelectedIds, deleteImportListExclusions, unselectAll]);

  const handleConfirmDeleteModalClose = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);
  }, []);

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setImportListExclusionSort({ sortKey, sortDirection });
    },
    []
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      if (payload.pageSize) {
        setImportListExclusionOption('pageSize', payload.pageSize as number);
        goToPage(1);
      }
    },
    [goToPage]
  );

  const [
    isAddImportListExclusionModalOpen,
    setAddImportListExclusionModalOpen,
    setAddImportListExclusionModalClosed,
  ] = useModalOpenState(false);

  const handleAddModalClose = useCallback(() => {
    setAddImportListExclusionModalClosed();
    refetch();
  }, [setAddImportListExclusionModalClosed, refetch]);

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
    <FieldSet legend={translate('ImportListExclusions')}>
      <PageSectionContent
        errorMessage={translate('ImportListExclusionsLoadError')}
        isFetching={isLoading && !isFetched}
        isPopulated={isFetched}
        error={error}
      >
        <Table
          selectAll={true}
          allSelected={allSelected}
          allUnselected={allUnselected}
          columns={COLUMNS}
          canModifyColumns={false}
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
                <ImportListExclusionRow
                  key={item.id}
                  {...item}
                  onModalClose={refetch}
                />
              );
            })}

            <TableRow>
              <TableRowCell colSpan={3}>
                <SpinnerButton
                  kind={kinds.DANGER}
                  isSpinning={isDeleting}
                  isDisabled={!anySelected}
                  onPress={handleDeleteSelectedPress}
                >
                  {translate('Delete')}
                </SpinnerButton>
              </TableRowCell>

              <TableRowCell>
                <IconButton
                  name={icons.ADD}
                  onPress={setAddImportListExclusionModalOpen}
                />
              </TableRowCell>
            </TableRow>
          </TableBody>
        </Table>

        <TablePager
          page={page}
          totalPages={totalPages}
          totalRecords={totalRecords}
          isFetching={isFetching}
          onPageSelect={goToPage}
        />

        <EditImportListExclusionModal
          isOpen={isAddImportListExclusionModalOpen}
          onModalClose={handleAddModalClose}
        />

        <ConfirmModal
          isOpen={isConfirmDeleteModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteSelected')}
          message={translate('DeleteSelectedImportListExclusionsMessageText')}
          confirmLabel={translate('DeleteSelected')}
          onConfirm={handleDeleteSelectedConfirmed}
          onCancel={handleConfirmDeleteModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

function ImportListExclusions() {
  const { records } = useImportListExclusions();

  return (
    <SelectProvider<ImportListExclusion> items={records}>
      <ImportListExclusionsContent />
    </SelectProvider>
  );
}

export default ImportListExclusions;
