import PropTypes from 'prop-types';
import React from 'react';
import { icons, sortDirections } from 'Helpers/Props';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Icon from 'Components/Icon';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import InteractiveEpisodeSearchRow from './InteractiveEpisodeSearchRow';

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
    items,
    sortKey,
    sortDirection,
    longDateFormat,
    timeFormat,
    onSortPress,
    onGrabPress
  } = props;

  if (isFetching) {
    return <LoadingIndicator />;
  } else if (!isFetching && !!error) {
    return <div>Unable to load results for this episode search. Try again later.</div>;
  } else if (isPopulated && !items.length) {
    return <div>No results found.</div>;
  }

  return (
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
  );
}

InteractiveEpisodeSearch.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string,
  sortDirection: PropTypes.string,
  longDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onGrabPress: PropTypes.func.isRequired
};

export default InteractiveEpisodeSearch;
