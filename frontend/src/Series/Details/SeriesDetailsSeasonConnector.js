import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand } from 'Utilities/Command';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { toggleSeasonMonitored } from 'Store/Actions/seriesActions';
import { toggleEpisodesMonitored, setEpisodesTableOption } from 'Store/Actions/episodeActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import SeriesDetailsSeason from './SeriesDetailsSeason';

function createMapStateToProps() {
  return createSelector(
    (state, { seasonNumber }) => seasonNumber,
    (state) => state.episodes,
    createSeriesSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (seasonNumber, episodes, series, commands, dimensions) => {
      const isSearching = !!findCommand(commands, {
        name: commandNames.SEASON_SEARCH,
        seriesId: series.id,
        seasonNumber
      });

      const episodesInSeason = _.filter(episodes.items, { seasonNumber });
      const sortedEpisodes = _.orderBy(episodesInSeason, 'episodeNumber', 'desc');

      return {
        items: sortedEpisodes,
        columns: episodes.columns,
        isSearching,
        seriesMonitored: series.monitored,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

const mapDispatchToProps = {
  toggleSeasonMonitored,
  toggleEpisodesMonitored,
  setEpisodesTableOption,
  executeCommand
};

class SeriesDetailsSeasonConnector extends Component {

  //
  // Listeners

  onTableOptionChange = (payload) => {
    this.props.setEpisodesTableOption(payload);
  }

  onMonitorSeasonPress = (monitored) => {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.toggleSeasonMonitored({
      seriesId,
      seasonNumber,
      monitored
    });
  }

  onSearchPress = () => {
    const {
      seriesId,
      seasonNumber
    } = this.props;

    this.props.executeCommand({
      name: commandNames.SEASON_SEARCH,
      seriesId,
      seasonNumber
    });
  }

  onMonitorEpisodePress = (episodeIds, monitored) => {
    this.props.toggleEpisodesMonitored({
      episodeIds,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <SeriesDetailsSeason
        {...this.props}
        onTableOptionChange={this.onTableOptionChange}
        onMonitorSeasonPress={this.onMonitorSeasonPress}
        onSearchPress={this.onSearchPress}
        onMonitorEpisodePress={this.onMonitorEpisodePress}
      />
    );
  }
}

SeriesDetailsSeasonConnector.propTypes = {
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  toggleSeasonMonitored: PropTypes.func.isRequired,
  toggleEpisodesMonitored: PropTypes.func.isRequired,
  setEpisodesTableOption: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesDetailsSeasonConnector);
