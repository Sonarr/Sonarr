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
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TablePager from 'Components/Table/TablePager';
import TableRow from 'Components/Table/TableRow';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { icons, kinds } from 'Helpers/Props';
import {
  bulkDeleteImportListExclusions,
  deleteImportListExclusion,
  fetchImportListExclusions,
  gotoImportListExclusionFirstPage,
  gotoImportListExclusionLastPage,
  gotoImportListExclusionNextPage,
  gotoImportListExclusionPage,
  gotoImportListExclusionPreviousPage,
  setImportListExclusionSort,
  setImportListExclusionTableOption,
} from 'Store/Actions/Settings/importListExclusions';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import EditImportListExclusionModal from './EditImportListExclusionModal';
import ImportListExclusionRow from './ImportListExclusionRow';

const COLUMNS = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
    isSortable: true,
  },
  {
    name: 'tvdbid',
    label: () => translate('TvdbId'),
    isVisible: true,
    isSortable: true,
  },
  {
    name: 'actions',
    isVisible: true,
    isSortable: false,
  },
];

function createImportListExlucionsSelector() {
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
    totalRecords,
    isDeleting,
    ...otherProps
  } = useSelector(createImportListExlucionsSelector());

  const dispatch = useDispatch();

  const [isConfirmRemoveModalOpen, setIsConfirmRemoveModalOpen] =
    useState(false);

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

  const handleRemoveSelectedPress = useCallback(() => {
    setIsConfirmRemoveModalOpen(true);
  }, [setIsConfirmRemoveModalOpen]);

  const handleConfirmRemoveModalClose = useCallback(() => {
    setIsConfirmRemoveModalOpen(false);
  }, [setIsConfirmRemoveModalOpen]);

  const gotoImportListExclusionFirstPagePress = useCallback(() => {
    dispatch(gotoImportListExclusionFirstPage());
  }, [dispatch]);

  const gotoImportListExclusionPreviousPagePress = useCallback(() => {
    dispatch(gotoImportListExclusionPreviousPage());
  }, [dispatch]);

  const gotoImportListExclusionNextPagePress = useCallback(() => {
    dispatch(gotoImportListExclusionNextPage());
  }, [dispatch]);

  const gotoImportListExclusionLastPagePress = useCallback(() => {
    dispatch(gotoImportListExclusionLastPage());
  }, [dispatch]);

  const gotoImportListExclusionPagePress = useCallback(
    (page: number) => {
      dispatch(gotoImportListExclusionPage({ page }));
    },
    [dispatch]
  );

  const onImportListExclusionSort = useCallback(
    (sortKey: { sortKey: string }) => {
      dispatch(setImportListExclusionSort({ sortKey }));
    },
    [dispatch]
  );

  const onImportListTableOption = useCallback(
    (payload: { pageSize: number }) => {
      dispatch(setImportListExclusionTableOption(payload));

      if (payload.pageSize) {
        dispatch(gotoImportListExclusionFirstPage());
      }
    },
    [dispatch]
  );

  const repopulate = useCallback(() => {
    gotoImportListExclusionFirstPagePress();
  }, [gotoImportListExclusionFirstPagePress]);

  useEffect(() => {
    registerPagePopulator(repopulate);

    if (requestCurrentPage) {
      dispatch(fetchImportListExclusions());
    } else {
      gotoImportListExclusionFirstPagePress();
    }

    return () => unregisterPagePopulator(repopulate);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const onConfirmDeleteImportListExclusion = useCallback(
    (id: number) => {
      dispatch(deleteImportListExclusion({ id }));
      repopulate();
    },
    [dispatch, repopulate]
  );

  const handleRemoveSelectedConfirmed = useCallback(() => {
    dispatch(bulkDeleteImportListExclusions({ ids: selectedIds }));
    setIsConfirmRemoveModalOpen(false);
    repopulate();
  }, [selectedIds, setIsConfirmRemoveModalOpen, dispatch, repopulate]);

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
          onTableOptionChange={onImportListTableOption}
          onSelectAllChange={handleSelectAllChange}
          onSortPress={onImportListExclusionSort}
        >
          <TableBody>
            {items.map((item) => {
              return (
                <ImportListExclusionRow
                  key={item.id}
                  {...item}
                  isSelected={selectedState[item.id] || false}
                  onConfirmDeleteImportListExclusion={
                    onConfirmDeleteImportListExclusion
                  }
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
                  onPress={handleRemoveSelectedPress}
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
          totalRecords={totalRecords}
          pageSize={pageSize}
          isFetching={isFetching}
          onFirstPagePress={gotoImportListExclusionFirstPagePress}
          onPreviousPagePress={gotoImportListExclusionPreviousPagePress}
          onNextPagePress={gotoImportListExclusionNextPagePress}
          onLastPagePress={gotoImportListExclusionLastPagePress}
          onPageSelect={gotoImportListExclusionPagePress}
          {...otherProps}
        />

        <EditImportListExclusionModal
          isOpen={isAddImportListExclusionModalOpen}
          onModalClose={setAddImportListExclusionModalClosed}
        />

        <ConfirmModal
          isOpen={isConfirmRemoveModalOpen}
          kind={kinds.DANGER}
          title={translate('RemoveSelected')}
          message={translate('RemoveSelectedImportListExclusionMessageText')}
          confirmLabel={translate('RemoveSelected')}
          onConfirm={handleRemoveSelectedConfirmed}
          onCancel={handleConfirmRemoveModalClose}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default ImportListExclusions;
