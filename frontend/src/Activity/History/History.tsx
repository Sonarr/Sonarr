import React, { useCallback, useEffect, useMemo, useState } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
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
import useEpisodes from 'Episode/useEpisodes';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import { align, icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import HistoryItem from 'typings/History';
import { TableOptionsChangePayload } from 'typings/Table';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import {
  registerPagePopulator,
  unregisterPagePopulator,
} from 'Utilities/pagePopulator';
import translate from 'Utilities/String/translate';
import HistoryFilterModal from './HistoryFilterModal';
import {
  setHistoryOption,
  setHistoryOptions,
  setHistorySort,
  useHistoryOptions,
} from './historyOptionsStore';
import HistoryRow from './HistoryRow';
import useHistory, { useFilters } from './useHistory';

function History() {
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
  } = useHistory();

  const { columns, pageSize, sortKey, sortDirection, selectedFilterKey } =
    useHistoryOptions();

  const episodeIds = useMemo(() => {
    return selectUniqueIds<HistoryItem, number>(records, 'episodeId');
  }, [records]);

  const {
    isFetching: isEpisodesFetching,
    isFetched: isEpisodesFetched,
    error: episodesError,
  } = useEpisodes({ episodeIds });

  const filters = useFilters();

  const customFilters = useCustomFiltersList('history');

  const isFetchingAny = isLoading || isEpisodesFetching;
  const isAllPopulated = isFetched && (isEpisodesFetched || !records.length);
  const hasError = error || episodesError;

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      setHistoryOption('selectedFilterKey', selectedFilterKey);
    },
    []
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setHistorySort({
        sortKey,
        sortDirection,
      });
    },
    []
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      setHistoryOptions(payload);

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
    <PageContent title={translate('History')}>
      <PageToolbar>
        <ToolbarItem
          id="refresh"
          priority={1}
          groupId="left"
          label={translate('Refresh')}
          iconName={icons.REFRESH}
          isSpinning={isFetching}
          onPress={handleRefreshPress}
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
            filterModalConnectorComponent={HistoryFilterModal}
            onFilterSelect={handleFilterSelect}
          />
        </ToolbarItem>
      </PageToolbar>

      <PageContentBody>
        <PageHeading
          scope={`${translate('Activity')} · ${translate('History')}`}
          title={translate('History')}
          meta={
            totalRecords > 0
              ? [translate('HistoryEventsCount', { count: totalRecords })]
              : [translate('HistoryNoEvents')]
          }
        />

        {isFetchingAny && !isAllPopulated ? <LoadingIndicator /> : null}

        {!isFetchingAny && hasError ? (
          <Alert kind={kinds.DANGER}>{translate('HistoryLoadError')}</Alert>
        ) : null}

        {
          // If history isPopulated and it's empty show no history found and don't
          // wait for the episodes to populate because they are never coming.

          isFetched && !hasError && !records.length ? (
            <PageMessage>{translate('NoHistoryFound')}</PageMessage>
          ) : null
        }

        {isAllPopulated && !hasError && records.length ? (
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
                    <HistoryRow key={item.id} columns={columns} {...item} />
                  );
                })}
              </TableBody>
            </Table>

            <TablePager
              page={page}
              totalPages={totalPages}
              totalRecords={totalRecords}
              showTotalRecords={false}
              isFetching={isFetching}
              onPageSelect={goToPage}
            />
          </div>
        ) : null}
      </PageContentBody>

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

export default History;
