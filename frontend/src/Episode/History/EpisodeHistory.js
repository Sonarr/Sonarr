import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons } from 'Helpers/Props';
import EpisodeHistoryRow from './EpisodeHistoryRow';

const columns = [
  {
    name: 'eventType',
    isVisible: true
  },
  {
    name: 'sourceTitle',
    label: 'Source Title',
    isVisible: true
  },
  {
    name: 'language',
    label: 'Language',
    isVisible: true
  },
  {
    name: 'quality',
    label: 'Quality',
    isVisible: true
  },
  {
    name: 'date',
    label: 'Date',
    isVisible: true
  },
  {
    name: 'details',
    label: 'Details',
    isVisible: true
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: 'Custom format score'
    }),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'actions',
    label: 'Actions',
    isVisible: true
  }
];

class EpisodeHistory extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      onMarkAsFailedPress
    } = this.props;

    const hasItems = !!items.length;

    if (isFetching) {
      return (
        <LoadingIndicator />
      );
    }

    if (!isFetching && !!error) {
      return (
        <div>Unable to load episode history.</div>
      );
    }

    if (isPopulated && !hasItems && !error) {
      return (
        <div>No episode history.</div>
      );
    }

    if (isPopulated && hasItems && !error) {
      return (
        <Table
          columns={columns}
        >
          <TableBody>
            {
              items.map((item) => {
                return (
                  <EpisodeHistoryRow
                    key={item.id}
                    {...item}
                    onMarkAsFailedPress={onMarkAsFailedPress}
                  />
                );
              })
            }
          </TableBody>
        </Table>
      );
    }

    return null;
  }
}

EpisodeHistory.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

EpisodeHistory.defaultProps = {
  selectedTab: 'details'
};

export default EpisodeHistory;
