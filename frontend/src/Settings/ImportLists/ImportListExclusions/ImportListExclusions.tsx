import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
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
import useSelectState from 'Helpers/Hooks/useSelectState';
import { icons, kinds } from 'Helpers/Props';
import {
  bulkDeleteImportListExclusions,
  clearImportListExclusions,
  fetchImportListExclusions,
  gotoImportListExclusionPage,
  setImportListExclusionSort,
  setImportListExclusionTableOption,
} from 'Store/Actions/Settings/importListExclusions';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import { TableOptionsChangePayload } from 'typings/Table';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
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

function ImportListExclusions() {
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

  const [selectState, setSelectState] = useSelectState();
  const { allSelected, allUnselected, selectedState } = selectState;

  const selectedIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

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

  const handleDeleteSelectedPress = useCallback(() => {
    setIsConfirmDeleteModalOpen(true);
  }, [setIsConfirmDeleteModalOpen]);

  const handleDeleteSelectedConfirmed = useCallback(() => {
    dispatch(bulkDeleteImportListExclusions({ ids: selectedIds }));
    setIsConfirmDeleteModalOpen(false);
  }, [selectedIds, setIsConfirmDeleteModalOpen, dispatch]);

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
    (sortKey: { sortKey: string }) => {
      dispatch(setImportListExclusionSort({ sortKey }));
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
      setSelectState({ type: 'unselectAll', items });

      dispatch(fetchImportListExclusions());
    }
  }, [
    previousIsDeleting,
    isDeleting,
    deleteError,
    items,
    dispatch,
    setSelectState,
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
              return (
                <ImportListExclusionRow
                  key={item.id}
                  {...item}
                  isSelected={selectedState[item.id] || false}
                  onSelectedChange={handleSelectedChange}
                />
              );
            })}

            <TableRow>
              <TableRowCell colSpan={3}>
                <SpinnerButton
                  kind={kinds.DANGER}
                  isSpinning={isDeleting}
                  isDisabled={!selectedIds.length}
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

export default ImportListExclusions;
