import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchSeriesHistory, clearSeriesHistory, seriesHistoryMarkAsFailed } from 'Store/Actions/seriesHistoryActions';
import SeriesHistoryModalContent from './SeriesHistoryModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.seriesHistory,
    (seriesHistory) => {
      return seriesHistory;
    }
  );
}

const mapDispatchToProps = {
  fetchSeriesHistory,
  clearSeriesHistory,
  seriesHistoryMarkAsFailed
};

class SeriesHistoryModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.fetchSeriesHistory({
      seriesId,
      seasonNumber
    });
  }

  componentWillUnmount() {
    this.props.clearSeriesHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.seriesHistoryMarkAsFailed({
      historyId,
      seriesId,
      seasonNumber
    });
  }

  //
  // Render

  render() {
    return (
      <SeriesHistoryModalContent
        {...this.props}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

SeriesHistoryModalContentConnector.propTypes = {
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number,
  fetchSeriesHistory: PropTypes.func.isRequired,
  clearSeriesHistory: PropTypes.func.isRequired,
  seriesHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesHistoryModalContentConnector);
