import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
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
import { align, icons, kinds } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { TableOptionsChangePayload } from 'typings/Table';
import translate from 'Utilities/String/translate';
import {
  setEventOption,
  setEventOptions,
  useEventOptions,
} from './eventOptionsStore';
import LogsTableRow from './LogsTableRow';
import useEvents, { useFilters } from './useEvents';

function LogsTable() {
  const dispatch = useDispatch();
  const {
    records,
    totalPages,
    totalRecords,
    error,
    isFetching,
    isFetched,
    isLoading,
    page,
    goToPage,
  } = useEvents();

  const { columns, pageSize, sortKey, sortDirection, selectedFilterKey } =
    useEventOptions();

  const filters = useFilters();

  const isClearLogExecuting = useSelector(
    createCommandExecutingSelector(commandNames.CLEAR_LOGS)
  );

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      setEventOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  const handleSortPress = useCallback((sortKey: string) => {
    setEventOption('sortKey', sortKey);
  }, []);

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      setEventOptions(payload);

      if (payload.pageSize) {
        goToPage(1);
      }
    },
    [goToPage]
  );

  const handleRefreshPress = useCallback(() => {
    goToPage(1);
  }, [goToPage]);

  const handleClearLogsPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: commandNames.CLEAR_LOGS,
        commandFinished: () => {
          goToPage(1);
        },
      })
    );
  }, [dispatch, goToPage]);

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
        {isLoading ? <LoadingIndicator /> : null}

        {isFetched && !error && !records.length ? (
          <Alert kind={kinds.INFO}>{translate('NoEventsFound')}</Alert>
        ) : null}

        {isFetched && !error && records.length ? (
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
                {records.map((item) => {
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
              onPageSelect={goToPage}
            />
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default LogsTable;
