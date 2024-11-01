import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import ReleasesAppState from 'App/State/ReleasesAppState';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { align, icons, kinds, sortDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import InteractiveSearchType from 'InteractiveSearch/InteractiveSearchType';
import {
  fetchReleases,
  grabRelease,
  setEpisodeReleasesFilter,
  setReleasesSort,
  setSeasonReleasesFilter,
} from 'Store/Actions/releaseActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import InteractiveSearchFilterModal from './InteractiveSearchFilterModal';
import InteractiveSearchRow from './InteractiveSearchRow';
import styles from './InteractiveSearch.css';

const columns: Column[] = [
  {
    name: 'protocol',
    label: () => translate('Source'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'age',
    label: () => translate('Age'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'title',
    label: () => translate('Title'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'indexer',
    label: () => translate('Indexer'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'peers',
    label: () => translate('Peers'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'languageWeight',
    label: () => translate('Languages'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'qualityWeight',
    label: () => translate('Quality'),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'indexerFlags',
    label: React.createElement(Icon, {
      name: icons.FLAG,
      title: () => translate('IndexerFlags'),
    }),
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, {
      name: icons.DANGER,
      title: () => translate('Rejections'),
    }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true,
  },
  {
    name: 'releaseWeight',
    label: React.createElement(Icon, { name: icons.DOWNLOAD }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true,
  },
];

interface InteractiveSearchProps {
  type: InteractiveSearchType;
  searchPayload: object;
}

function InteractiveSearch({ type, searchPayload }: InteractiveSearchProps) {
  const {
    isFetching,
    isPopulated,
    error,
    items,
    totalItems,
    selectedFilterKey,
    filters,
    customFilters,
    sortKey,
    sortDirection,
  }: ReleasesAppState & ClientSideCollectionAppState = useSelector(
    createClientSideCollectionSelector('releases', `releases.${type}`)
  );

  const dispatch = useDispatch();

  const handleFilterSelect = useCallback(
    (selectedFilterKey: string) => {
      const action =
        type === 'episode' ? setEpisodeReleasesFilter : setSeasonReleasesFilter;

      dispatch(action({ selectedFilterKey }));
    },
    [type, dispatch]
  );

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection: SortDirection) => {
      dispatch(setReleasesSort({ sortKey, sortDirection }));
    },
    [dispatch]
  );

  const handleGrabPress = useCallback(
    (payload: object) => {
      dispatch(grabRelease(payload));
    },
    [dispatch]
  );

  useEffect(
    () => {
      // Only fetch releases if they are not already being fetched and not yet populated.

      if (!isFetching && !isPopulated) {
        dispatch(fetchReleases(searchPayload));
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  const errorMessage = getErrorMessage(error);

  return (
    <div>
      <div className={styles.filterMenuContainer}>
        <FilterMenu
          alignMenu={align.RIGHT}
          selectedFilterKey={selectedFilterKey}
          filters={filters}
          customFilters={customFilters}
          buttonComponent={PageMenuButton}
          filterModalConnectorComponent={InteractiveSearchFilterModal}
          filterModalConnectorComponentProps={{ type }}
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

      {!isFetching && isPopulated && !totalItems ? (
        <Alert kind={kinds.INFO}>{translate('NoResultsFound')}</Alert>
      ) : null}

      {!!totalItems && isPopulated && !items.length ? (
        <Alert kind={kinds.WARNING}>
          {translate('AllResultsAreHiddenByTheAppliedFilter')}
        </Alert>
      ) : null}

      {isPopulated && !!items.length ? (
        <Table
          columns={columns}
          sortKey={sortKey}
          sortDirection={sortDirection}
          onSortPress={handleSortPress}
        >
          <TableBody>
            {items.map((item) => {
              return (
                <InteractiveSearchRow
                  key={`${item.indexerId}-${item.guid}`}
                  {...item}
                  searchPayload={searchPayload}
                  onGrabPress={handleGrabPress}
                />
              );
            })}
          </TableBody>
        </Table>
      ) : null}

      {totalItems !== items.length && !!items.length ? (
        <div className={styles.filteredMessage}>
          {translate('SomeResultsAreHiddenByTheAppliedFilter')}
        </div>
      ) : null}
    </div>
  );
}

export default InteractiveSearch;
