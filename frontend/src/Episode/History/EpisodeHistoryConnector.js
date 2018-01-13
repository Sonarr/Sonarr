import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchEpisodeHistory, clearEpisodeHistory, episodeHistoryMarkAsFailed } from 'Store/Actions/episodeHistoryActions';
import EpisodeHistory from './EpisodeHistory';

function createMapStateToProps() {
  return createSelector(
    (state) => state.episodeHistory,
    (episodeHistory) => {
      return episodeHistory;
    }
  );
}

const mapDispatchToProps = {
  fetchEpisodeHistory,
  clearEpisodeHistory,
  episodeHistoryMarkAsFailed
};

class EpisodeHistoryConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchEpisodeHistory({ episodeId: this.props.episodeId });
  }

  componentWillUnmount() {
    this.props.clearEpisodeHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    this.props.episodeHistoryMarkAsFailed({ historyId, episodeId: this.props.episodeId });
  }

  //
  // Render

  render() {
    return (
      <EpisodeHistory
        {...this.props}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

EpisodeHistoryConnector.propTypes = {
  episodeId: PropTypes.number.isRequired,
  fetchEpisodeHistory: PropTypes.func.isRequired,
  clearEpisodeHistory: PropTypes.func.isRequired,
  episodeHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EpisodeHistoryConnector);
