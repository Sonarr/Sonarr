import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
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
import createEpisodesFetchingSelector from 'Episode/createEpisodesFetchingSelector';
import { useCustomFiltersList } from 'Filters/useCustomFilters';
import useCurrentPage from 'Helpers/Hooks/useCurrentPage';
import { align, icons, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { clearEpisodes, fetchEpisodes } from 'Store/Actions/episodeActions';
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

  const filters = useFilters();

  const requestCurrentPage = useCurrentPage();

  const { isEpisodesFetching, isEpisodesPopulated, episodesError } =
    useSelector(createEpisodesFetchingSelector());
  const customFilters = useCustomFiltersList('history');
  const dispatch = useDispatch();

  const isFetchingAny = isLoading || isEpisodesFetching;
  const isAllPopulated = isFetched && (isEpisodesPopulated || !records.length);
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

  useEffect(() => {
    return () => {
      dispatch(clearEpisodes());
    };
  }, [requestCurrentPage, dispatch]);

  useEffect(() => {
    const episodeIds = selectUniqueIds<HistoryItem, number>(
      records,
      'episodeId'
    );

    if (episodeIds.length) {
      dispatch(fetchEpisodes({ episodeIds }));
    } else {
      dispatch(clearEpisodes());
    }
  }, [records, dispatch]);

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
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('Refresh')}
            iconName={icons.REFRESH}
            isSpinning={isFetching}
            onPress={handleRefreshPress}
          />
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT}>
          <TableOptionsModalWrapper
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
            customFilters={customFilters}
            filterModalConnectorComponent={HistoryFilterModal}
            onFilterSelect={handleFilterSelect}
          />
        </PageToolbarSection>
      </PageToolbar>

      <PageContentBody>
        {isFetchingAny && !isAllPopulated ? <LoadingIndicator /> : null}

        {!isFetchingAny && hasError ? (
          <Alert kind={kinds.DANGER}>{translate('HistoryLoadError')}</Alert>
        ) : null}

        {
          // If history isPopulated and it's empty show no history found and don't
          // wait for the episodes to populate because they are never coming.

          isFetched && !hasError && !records.length ? (
            <Alert kind={kinds.INFO}>{translate('NoHistoryFound')}</Alert>
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
              isFetching={isFetching}
              onPageSelect={goToPage}
            />
          </div>
        ) : null}
      </PageContentBody>
    </PageContent>
  );
}

export default History;
