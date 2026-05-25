import React, { useCallback, useMemo, useState } from 'react';
import CommandNames from 'Commands/CommandNames';
import { useCommandExecuting, useExecuteCommand } from 'Commands/useCommands';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar, {
  type MoreMenuItem,
} from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSpacer from 'Components/Page/Toolbar/PageToolbarSpacer';
import ToolbarItem from 'Components/Page/Toolbar/ToolbarItem';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
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

  const onTableOptionsPress = useCallback(() => {
    setIsTableOptionsModalOpen(true);
  }, []);

  const onTableOptionsModalClose = useCallback(() => {
    setIsTableOptionsModalOpen(false);
  }, []);

  const moreMenuItems = useMemo<MoreMenuItem[]>(
    () => [
      {
        id: 'refresh',
        label: translate('Refresh'),
        iconName: icons.REFRESH,
        isSpinning: isFetching,
        onPress: handleRefreshPress,
      },
      {
        id: 'clear',
        label: translate('Clear'),
        iconName: icons.CLEAR,
        isSpinning: isClearLogExecuting,
        onPress: handleClearLogsPress,
      },
      {
        id: 'options',
        label: translate('Options'),
        iconName: icons.TABLE,
        onPress: onTableOptionsPress,
      },
    ],
    [
      isFetching,
      handleRefreshPress,
      isClearLogExecuting,
      handleClearLogsPress,
      onTableOptionsPress,
    ]
  );

  return (
    <PageContent title={translate('Logs')}>
      <PageToolbar moreMenuItems={moreMenuItems}>
        <ToolbarItem id="refresh" priority={1} groupId="left-a">
          <PageToolbarButton
            label={translate('Refresh')}
            iconName={icons.REFRESH}
            spinningName={icons.REFRESH}
            isSpinning={isFetching}
            onPress={handleRefreshPress}
          />
        </ToolbarItem>

        <ToolbarItem id="clear" priority={1} groupId="left-a">
          <PageToolbarButton
            label={translate('Clear')}
            iconName={icons.CLEAR}
            isSpinning={isClearLogExecuting}
            onPress={handleClearLogsPress}
          />
        </ToolbarItem>

        <PageToolbarSpacer />

        <ToolbarItem id="options" priority={2} groupId="right-a">
          <TableOptionsModalWrapper
            canModifyColumns={false}
            columns={columns}
            pageSize={pageSize}
            isOpen={isTableOptionsModalOpen}
            onPress={onTableOptionsPress}
            onModalClose={onTableOptionsModalClose}
            onTableOptionChange={handleTableOptionChange}
          >
            <PageToolbarButton
              label={translate('Options')}
              iconName={icons.TABLE}
            />
          </TableOptionsModalWrapper>
        </ToolbarItem>

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
    </PageContent>
  );
}

export default LogsTable;
