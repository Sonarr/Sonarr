import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import { toggleSeriesMonitored, toggleSeasonMonitored } from 'Store/Actions/seriesActions';
import SeasonPassRow from './SeasonPassRow';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    (series) => {
      return _.pick(series, [
        'status',
        'titleSlug',
        'title',
        'monitored',
        'seasons',
        'isSaving'
      ]);
    }
  );
}

const mapDispatchToProps = {
  toggleSeriesMonitored,
  toggleSeasonMonitored
};

class SeasonPassRowConnector extends Component {

  //
  // Listeners

  onSeriesMonitoredPress = () => {
    const {
      seriesId,
      monitored
    } = this.props;

    this.props.toggleSeriesMonitored({
      seriesId,
      monitored: !monitored
    });
  }

  onSeasonMonitoredPress = (seasonNumber, monitored) => {
    this.props.toggleSeasonMonitored({
      seriesId: this.props.seriesId,
      seasonNumber,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <SeasonPassRow
        {...this.props}
        onSeriesMonitoredPress={this.onSeriesMonitoredPress}
        onSeasonMonitoredPress={this.onSeasonMonitoredPress}
      />
    );
  }
}

SeasonPassRowConnector.propTypes = {
  seriesId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  toggleSeriesMonitored: PropTypes.func.isRequired,
  toggleSeasonMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeasonPassRowConnector);
