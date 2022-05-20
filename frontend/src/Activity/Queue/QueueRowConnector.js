import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { grabQueueItem, removeQueueItem } from 'Store/Actions/queueActions';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import QueueRow from './QueueRow';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createEpisodeSelector(),
    createUISettingsSelector(),
    (series, episode, uiSettings) => {
      const result = {
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat
      };

      result.series = series;
      result.episode = episode;

      return result;
    }
  );
}

const mapDispatchToProps = {
  grabQueueItem,
  removeQueueItem
};

class QueueRowConnector extends Component {

  //
  // Listeners

  onGrabPress = () => {
    this.props.grabQueueItem({ id: this.props.id });
  };

  onRemoveQueueItemPress = (payload) => {
    this.props.removeQueueItem({ id: this.props.id, ...payload });
  };

  //
  // Render

  render() {
    return (
      <QueueRow
        {...this.props}
        onGrabPress={this.onGrabPress}
        onRemoveQueueItemPress={this.onRemoveQueueItemPress}
      />
    );
  }
}

QueueRowConnector.propTypes = {
  id: PropTypes.number.isRequired,
  episode: PropTypes.object,
  grabQueueItem: PropTypes.func.isRequired,
  removeQueueItem: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(QueueRowConnector);
