import PropTypes from 'prop-types';
import React from 'react';
import { align, icons, sortDirections } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Icon from 'Components/Icon';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import InteractiveSearchFilterModalConnector from './InteractiveSearchFilterModalConnector';
import InteractiveEpisodeSearchRow from './InteractiveEpisodeSearchRow';
import styles from './InteractiveEpisodeSearch.css';

const columns = [
  {
    name: 'protocol',
    label: 'Source',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'age',
    label: 'Age',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'title',
    label: 'Title',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'indexer',
    label: 'Indexer',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'size',
    label: 'Size',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'peers',
    label: 'Peers',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'qualityWeight',
    label: 'Quality',
    isSortable: true,
    isVisible: true
  },
  {
    name: 'rejections',
    label: React.createElement(Icon, { name: icons.DANGER }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true
  },
  {
    name: 'releaseWeight',
    label: React.createElement(Icon, { name: icons.DOWNLOAD }),
    isSortable: true,
    fixedSortDirection: sortDirections.ASCENDING,
    isVisible: true
  }
];

function InteractiveEpisodeSearch(props) {
  const {
    isFetching,
    isPopulated,
    error,
    totalReleasesCount,
    items,
    selectedFilterKey,
    filters,
    customFilters,
    sortKey,
    sortDirection,
    longDateFormat,
    timeFormat,
    onSortPress,
    onFilterSelect,
    onGrabPress
  } = props;

  return (
    <div>
      <div className={styles.filterMenuContainer}>
        <FilterMenu
          alignMenu={align.RIGHT}
          selectedFilterKey={selectedFilterKey}
          filters={filters}
          customFilters={customFilters}
          buttonComponent={PageMenuButton}
          filterModalConnectorComponent={InteractiveSearchFilterModalConnector}
          onFilterSelect={onFilterSelect}
        />
      </div>

      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && !!error &&
          <div>
            Unable to load results for this episode search. Try again later.
          </div>
      }

      {
        !isFetching && isPopulated && !totalReleasesCount &&
          <div>
            No results found.
          </div>
      }

      {
        !!totalReleasesCount && isPopulated && !items.length &&
          <div>
            All results are hidden by {filters.length > 1 ? 'filters' : 'a filter'}.
          </div>
      }

      {
        isPopulated && !!items.length &&
          <Table
            columns={columns}
            sortKey={sortKey}
            sortDirection={sortDirection}
            onSortPress={onSortPress}
          >
            <TableBody>
              {
                items.map((item) => {
                  return (
                    <InteractiveEpisodeSearchRow
                      key={item.guid}
                      {...item}
                      longDateFormat={longDateFormat}
                      timeFormat={timeFormat}
                      onGrabPress={onGrabPress}
                    />
                  );
                })
              }
            </TableBody>
          </Table>
      }

      {
        totalReleasesCount !== items.length && !!items.length &&
          <div>
                Some results are hidden by {filters.length > 1 ? 'filters' : 'a filter'}.
          </div>
      }
    </div>
  );
}

InteractiveEpisodeSearch.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  totalReleasesCount: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectedFilterKey: PropTypes.string.isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onGrabPress: PropTypes.func.isRequired
};

export default InteractiveEpisodeSearch;
