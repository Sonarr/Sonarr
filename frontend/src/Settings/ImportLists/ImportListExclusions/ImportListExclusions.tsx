import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useHistory } from 'react-router';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FieldSet from 'Components/FieldSet';
import IconButton from 'Components/Link/IconButton';
import PageSectionContent from 'Components/Page/PageSectionContent';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TablePager from 'Components/Table/TablePager';
import TableRow from 'Components/Table/TableRow';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { icons } from 'Helpers/Props';
import * as importListExclusionActions from 'Store/Actions/Settings/importListExclusions';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
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
  const history = useHistory();
  const useCurrentPage = history.action === 'POP';

  const dispatch = useDispatch();

  const fetchImportListExclusions = useCallback(() => {
    dispatch(importListExclusionActions.fetchImportListExclusions());
  }, [dispatch]);

  const deleteImportListExclusion = useCallback(
    (payload: { id: number }) => {
      dispatch(importListExclusionActions.deleteImportListExclusion(payload));
    },
    [dispatch]
  );

  const gotoImportListExclusionFirstPage = useCallback(() => {
    dispatch(importListExclusionActions.gotoImportListExclusionFirstPage());
  }, [dispatch]);

  const gotoImportListExclusionPreviousPage = useCallback(() => {
    dispatch(importListExclusionActions.gotoImportListExclusionPreviousPage());
  }, [dispatch]);

  const gotoImportListExclusionNextPage = useCallback(() => {
    dispatch(importListExclusionActions.gotoImportListExclusionNextPage());
  }, [dispatch]);

  const gotoImportListExclusionLastPage = useCallback(() => {
    dispatch(importListExclusionActions.gotoImportListExclusionLastPage());
  }, [dispatch]);

  const gotoImportListExclusionPage = useCallback(
    (page: number) => {
      dispatch(
        importListExclusionActions.gotoImportListExclusionPage({ page })
      );
    },
    [dispatch]
  );

  const setImportListExclusionSort = useCallback(
    (sortKey: { sortKey: string }) => {
      dispatch(
        importListExclusionActions.setImportListExclusionSort({ sortKey })
      );
    },
    [dispatch]
  );

  const setImportListTableOption = useCallback(
    (payload: { pageSize: number }) => {
      dispatch(
        importListExclusionActions.setImportListExclusionTableOption(payload)
      );

      if (payload.pageSize) {
        dispatch(importListExclusionActions.gotoImportListExclusionFirstPage());
      }
    },
    [dispatch]
  );

  const repopulate = useCallback(() => {
    gotoImportListExclusionFirstPage();
  }, [gotoImportListExclusionFirstPage]);

  useEffect(() => {
    registerPagePopulator(repopulate);

    if (useCurrentPage) {
      fetchImportListExclusions();
    } else {
      gotoImportListExclusionFirstPage();
    }

    return () => unregisterPagePopulator(repopulate);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const onConfirmDeleteImportListExclusion = useCallback(
    (id: number) => {
      deleteImportListExclusion({ id });
      repopulate();
    },
    [deleteImportListExclusion, repopulate]
  );

  const selected = useSelector(createImportListExlucionsSelector());

  const {
    isFetching,
    isPopulated,
    items,
    pageSize,
    sortKey,
    error,
    sortDirection,
    totalRecords,
    ...otherProps
  } = selected;

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
          columns={COLUMNS}
          canModifyColumns={false}
          pageSize={pageSize}
          sortKey={sortKey}
          sortDirection={sortDirection}
          onSortPress={setImportListExclusionSort}
          onTableOptionChange={setImportListTableOption}
        >
          <TableBody>
            {items.map((item) => {
              return (
                <ImportListExclusionRow
                  key={item.id}
                  {...item}
                  onConfirmDeleteImportListExclusion={
                    onConfirmDeleteImportListExclusion
                  }
                />
              );
            })}

            <TableRow>
              <TableRowCell />
              <TableRowCell />

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
          onFirstPagePress={gotoImportListExclusionFirstPage}
          onPreviousPagePress={gotoImportListExclusionPreviousPage}
          onNextPagePress={gotoImportListExclusionNextPage}
          onLastPagePress={gotoImportListExclusionLastPage}
          onPageSelect={gotoImportListExclusionPage}
          {...otherProps}
        />

        <EditImportListExclusionModal
          isOpen={isAddImportListExclusionModalOpen}
          onModalClose={setAddImportListExclusionModalClosed}
        />
      </PageSectionContent>
    </FieldSet>
  );
}

export default ImportListExclusions;
