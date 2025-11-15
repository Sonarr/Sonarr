import React, { useCallback } from 'react';
import { useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { align, kinds } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import { createCustomFiltersSelector } from 'Store/Selectors/createClientSideCollectionSelector';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import InteractiveSearchFilterModal from './InteractiveSearchFilterModal';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import InteractiveSearchRow from './InteractiveSearchRow';
import InteractiveSearchType from './InteractiveSearchType';
import { setReleaseOption, useReleaseOptions } from './releaseOptionsStore';
import useReleases, { FILTERS, setReleaseSort } from './useReleases';
import styles from './InteractiveSearch.css';

interface InteractiveSearchProps {
  type: InteractiveSearchType;
  searchPayload: InteractiveSearchPayload;
}

function InteractiveSearch({ type, searchPayload }: InteractiveSearchProps) {
  const customFilters = useSelector(createCustomFiltersSelector('releases'));
  const { columns } = useReleaseOptions();

  const {
    isFetching,
    isFetched,
    error,
    data,
    totalItems,
    selectedFilterKey,
    sortKey,
    sortDirection,
  } = useReleases(searchPayload);

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string | number) => {
      if (type === 'episode') {
        setReleaseOption('episodeSelectedFilterKey', selectedFilterKey);
      } else {
        setReleaseOption('seasonSelectedFilterKey', selectedFilterKey);
      }
    },
    [type]
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setReleaseSort(sortKey, sortDirection);
    },
    []
  );

  const errorMessage = getErrorMessage(error);

  return (
    <div>
      <div className={styles.filterMenuContainer}>
        <FilterMenu
          alignMenu={align.RIGHT}
          selectedFilterKey={selectedFilterKey}
          filters={FILTERS}
          customFilters={customFilters}
          buttonComponent={PageMenuButton}
          filterModalConnectorComponent={InteractiveSearchFilterModal}
          filterModalConnectorComponentProps={{ type, searchPayload }}
          onFilterSelect={handleFilterSelect}
        />
      </div>

      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <div>
          {errorMessage ? (
            <>
              {translate('InteractiveSearchResultsSeriesFailedErrorMessage', {
                message:
                  errorMessage.charAt(0).toLowerCase() + errorMessage.slice(1),
              })}
            </>
          ) : (
            translate('EpisodeSearchResultsLoadError')
          )}
        </div>
      ) : null}

      {!isFetching && isFetched && !totalItems ? (
        <Alert kind={kinds.INFO}>{translate('NoResultsFound')}</Alert>
      ) : null}

      {!!totalItems && !isFetching && !data.length ? (
        <Alert kind={kinds.WARNING}>
          {translate('AllResultsAreHiddenByTheAppliedFilter')}
        </Alert>
      ) : null}

      {!isFetching && !!data.length ? (
        <Table
          columns={columns}
          sortKey={sortKey}
          sortDirection={sortDirection}
          onSortPress={handleSortPress}
        >
          <TableBody>
            {data.map((item) => {
              return (
                <InteractiveSearchRow
                  key={`${item.release.indexerId}-${item.release.guid}`}
                  {...item}
                  searchPayload={searchPayload}
                />
              );
            })}
          </TableBody>
        </Table>
      ) : null}

      {!isFetching && totalItems !== data.length && !!data.length ? (
        <div className={styles.filteredMessage}>
          {translate('SomeResultsAreHiddenByTheAppliedFilter')}
        </div>
      ) : null}
    </div>
  );
}

export default InteractiveSearch;
