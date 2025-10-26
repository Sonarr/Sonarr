import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import AppState from 'App/State/AppState';
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
import usePaging from 'Components/Table/usePaging';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import {
  bulkDeleteImportListExclusions,
  clearImportListExclusions,
  fetchImportListExclusions,
  gotoImportListExclusionPage,
  setImportListExclusionSort,
  setImportListExclusionTableOption,
} from 'Store/Actions/Settings/importListExclusions';
import ImportListExclusion from 'typings/ImportListExclusion';
import { CheckInputChanged } from 'typings/inputs';
import { TableOptionsChangePayload } from 'typings/Table';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModal from './EditImportListExclusionModal';
import ImportListExclusionRow from './ImportListExclusionRow';
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

function createImportListExclusionsSelector() {
  return createSelector(
    (state: AppState) => state.settings.importListExclusions,
    (importListExclusions) => {
      return {
        ...importListExclusions,
      };
    }
  );
}

function ImportListExclusionsContent() {
  const requestCurrentPage = useCurrentPage();

  const {
    isFetching,
    isPopulated,
    items,
    pageSize,
    sortKey,
    error,
    sortDirection,
    page,
    totalPages,
    totalRecords,
    isDeleting,
    deleteError,
  } = useSelector(createImportListExclusionsSelector());

  const dispatch = useDispatch();

  const [isConfirmDeleteModalOpen, setIsConfirmDeleteModalOpen] =
    useState(false);
  const previousIsDeleting = usePrevious(isDeleting);

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
  }, [setIsConfirmDeleteModalOpen]);

  const handleDeleteSelectedConfirmed = useCallback(() => {
    dispatch(bulkDeleteImportListExclusions({ ids: getSelectedIds() }));
    setIsConfirmDeleteModalOpen(false);
  }, [getSelectedIds, setIsConfirmDeleteModalOpen, dispatch]);

  const handleConfirmDeleteModalClose = useCallback(() => {
    setIsConfirmDeleteModalOpen(false);
  }, [setIsConfirmDeleteModalOpen]);

  const {
    handleFirstPagePress,
    handlePreviousPagePress,
    handleNextPagePress,
    handleLastPagePress,
    handlePageSelect,
  } = usePaging({
    page,
    totalPages,
    gotoPage: gotoImportListExclusionPage,
  });

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      dispatch(setImportListExclusionSort({ sortKey, sortDirection }));
    },
    [dispatch]
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      dispatch(setImportListExclusionTableOption(payload));

      if (payload.pageSize) {
        dispatch(gotoImportListExclusionPage({ page: 1 }));
      }
    },
    [dispatch]
  );

  useEffect(() => {
    if (requestCurrentPage) {
      dispatch(fetchImportListExclusions());
    } else {
      dispatch(gotoImportListExclusionPage({ page: 1 }));
    }

    return () => {
      dispatch(clearImportListExclusions());
    };
  }, [requestCurrentPage, dispatch]);

  useEffect(() => {
    const repopulate = () => {
      dispatch(fetchImportListExclusions());
    };

    registerPagePopulator(repopulate);

    return () => {
      unregisterPagePopulator(repopulate);
    };
  }, [dispatch]);

  useEffect(() => {
    if (previousIsDeleting && !isDeleting && !deleteError) {
      unselectAll();

      dispatch(fetchImportListExclusions());
    }
  }, [
    previousIsDeleting,
    isDeleting,
    deleteError,
    items,
    dispatch,
    unselectAll,
  ]);

  const [
    isAddImportListExclusionModalOpen,
    setAddImportListExclusionModalOpen,
    setAddImportListExclusionModalClosed,
  ] = useModalOpenState(false);

  const isFetchingForFirstTime = isFetching && !isPopulated;

  return (
    <FieldSet legend={translate('ImportListExclusions')}>
      <PageSectionContent
        errorMessage={translate('ImportListExclusionsLoadError')}
        isFetching={isFetchingForFirstTime}
        isPopulated={isPopulated}
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
            {items.map((item) => {
              return <ImportListExclusionRow key={item.id} {...item} />;
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
          onFirstPagePress={handleFirstPagePress}
          onPreviousPagePress={handlePreviousPagePress}
          onNextPagePress={handleNextPagePress}
          onLastPagePress={handleLastPagePress}
          onPageSelect={handlePageSelect}
        />

        <EditImportListExclusionModal
          isOpen={isAddImportListExclusionModalOpen}
          onModalClose={setAddImportListExclusionModalClosed}
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
  const { items } = useSelector(createImportListExclusionsSelector());

  return (
    <SelectProvider items={items}>
      <ImportListExclusionsContent />
    </SelectProvider>
  );
}

export default ImportListExclusions;
