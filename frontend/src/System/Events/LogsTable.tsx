import React, { useCallback, useEffect } from 'react';
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
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import TablePager from 'Components/Table/TablePager';
import usePaging from 'Components/Table/usePaging';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import { align, icons, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  fetchLogs,
  gotoLogsFirstPage,
  gotoLogsPage,
  setLogsFilter,
  setLogsSort,
  setLogsTableOption,
} from 'Store/Actions/systemActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { TableOptionsChangePayload } from 'typings/Table';
import translate from 'Utilities/String/translate';
import LogsTableRow from './LogsTableRow';

function LogsTable() {
  const dispatch = useDispatch();
  const requestCurrentPage = useCurrentPage();

  const {
    isFetching,
    isPopulated,
    error,
    items,
    columns,
    page,
    pageSize,
    totalPages,
    totalRecords,
    sortKey,
    sortDirection,
    filters,
    selectedFilterKey,
  } = useSelector((state: AppState) => state.system.logs);

  const isClearLogExecuting = useSelector(
    createCommandExecutingSelector(commandNames.CLEAR_LOGS)
  );

  const {
    handleFirstPagePress,
    handlePreviousPagePress,
    handleNextPagePress,
    handleLastPagePress,
    handlePageSelect,
  } = usePaging({
    page,
    totalPages,
    gotoPage: gotoLogsPage,
  });

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      dispatch(setLogsFilter({ selectedFilterKey }));
    },
    [dispatch]
  );

  const handleSortPress = useCallback(
    (sortKey: string) => {
      dispatch(setLogsSort({ sortKey }));
    },
    [dispatch]
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      dispatch(setLogsTableOption(payload));

      if (payload.pageSize) {
        dispatch(gotoLogsFirstPage({ page: 1 }));
      }
    },
    [dispatch]
  );

  const handleRefreshPress = useCallback(() => {
    dispatch(gotoLogsFirstPage());
  }, [dispatch]);

  const handleClearLogsPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.CLEAR_LOGS,
        commandFinished: () => {
          dispatch(gotoLogsFirstPage());
        },
      })
    );
  }, [dispatch]);

  useEffect(() => {
    if (requestCurrentPage) {
      dispatch(fetchLogs());
    } else {
      dispatch(gotoLogsFirstPage({ page: 1 }));
    }
  }, [requestCurrentPage, dispatch]);

  return (
    <PageContent title={translate('Logs')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('Refresh')}
            iconName={icons.REFRESH}
            spinningName={icons.REFRESH}
            isSpinning={isFetching}
            onPress={handleRefreshPress}
          />

          <PageToolbarButton
            label={translate('Clear')}
            iconName={icons.CLEAR}
            isSpinning={isClearLogExecuting}
            onPress={handleClearLogsPress}
          />
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT}>
          <TableOptionsModalWrapper
            canModifyColumns={false}
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

        {isPopulated && !error && !items.length ? (
          <Alert kind={kinds.INFO}>{translate('NoEventsFound')}</Alert>
        ) : null}

        {isPopulated && !error && items.length ? (
          <div>
            <Table
              columns={columns}
              pageSize={pageSize}
              sortKey={sortKey}
              sortDirection={sortDirection}
              onTableOptionChange={handleTableOptionChange}
              onSortPress={handleSortPress}
            >
              <TableBody>
                {items.map((item) => {
                  return (
                    <LogsTableRow key={item.id} columns={columns} {...item} />
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
    </PageContent>
  );
}

export default LogsTable;
