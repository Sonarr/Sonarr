import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EpisodeHistoryRow from './EpisodeHistoryRow';

const columns = [
  {
    name: 'eventType',
    isVisible: true
  },
  {
    name: 'sourceTitle',
    label: () => translate('SourceTitle'),
    isVisible: true
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isVisible: true
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isVisible: true
  },
  {
    name: 'customFormats',
    label: () => translate('CustomFormats'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'customFormatScore',
    label: React.createElement(Icon, {
      name: icons.SCORE,
      title: () => translate('CustomFormatScore')
    }),
    isSortable: true,
    isVisible: true
  },
  {
    name: 'date',
    label: () => translate('Date'),
    isVisible: true
  },
  {
    name: 'actions',
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
        <Alert kind={kinds.DANGER}>{translate('EpisodeHistoryLoadError')}</Alert>
      );
    }

    if (isPopulated && !hasItems && !error) {
      return (
        <Alert kind={kinds.INFO}>{translate('NoEpisodeHistory')}</Alert>
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
