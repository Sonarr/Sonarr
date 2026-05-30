import React, { useCallback, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModal from 'Components/Table/TableOptions/TableOptionsModal';
import TablePager from 'Components/Table/TablePager';
import { align, icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { TableOptionsChangePayload } from 'typings/Table';
import translate from 'Utilities/String/translate';
import {
  setEventOption,
  setEventOptions,
  setEventSort,
  useEventOptions,
} from './eventOptionsStore';
import LogsTableRow from './LogsTableRow';
import useEvents, { useFilters } from './useEvents';

function LogsTable() {
  const executeCommand = useExecuteCommand();
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
    refetch,
  } = useEvents();

  const { columns, pageSize, sortKey, sortDirection, selectedFilterKey } =
    useEventOptions();

  const filters = useFilters();

  const isClearLogExecuting = useCommandExecuting(CommandNames.ClearLog);

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      setEventOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setEventSort({
        sortKey,
        sortDirection,
      });
    },
    []
  );

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
    refetch();
  }, [goToPage, refetch]);

  const handleClearLogsPress = useCallback(() => {
    executeCommand({ name: CommandNames.ClearLog }, () => {
      goToPage(1);
      refetch();
    });
  }, [executeCommand, goToPage, refetch]);

  const [isTableOptionsModalOpen, setIsTableOptionsModalOpen] = useState(false);

  const handleTableOptionsPress = useCallback(() => {
    setIsTableOptionsModalOpen(true);
  }, []);

  const handleTableOptionsModalClose = useCallback(() => {
    setIsTableOptionsModalOpen(false);
  }, []);

  return (
    <PageContent title={translate('Logs')}>
      <PageToolbar>
        <ToolbarItem
          id="refresh"
          priority={1}
          groupId="left"
          label={translate('Refresh')}
          iconName={icons.REFRESH}
          spinningName={icons.REFRESH}
          isSpinning={isFetching}
          onPress={handleRefreshPress}
        />

        <ToolbarItem
          id="clear"
          priority={1}
          groupId="left"
          label={translate('Clear')}
          iconName={icons.CLEAR}
          isSpinning={isClearLogExecuting}
          onPress={handleClearLogsPress}
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
            customFilters={[]}
            onFilterSelect={handleFilterSelect}
          />
        </ToolbarItem>
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

      <TableOptionsModal
        isOpen={isTableOptionsModalOpen}
        canModifyColumns={false}
        columns={columns}
        pageSize={pageSize}
        onTableOptionChange={handleTableOptionChange}
        onModalClose={handleTableOptionsModalClose}
      />
    </PageContent>
  );
}

export default LogsTable;
