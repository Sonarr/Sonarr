import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { setEpisodesSort, setEpisodesTableOption, toggleEpisodesMonitored } from 'Store/Actions/episodeActions';
import { toggleSeasonMonitored } from 'Store/Actions/seriesActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import SeriesDetailsSeason from './SeriesDetailsSeason';

function createMapStateToProps() {
  return createSelector(
    (state, { seasonNumber }) => seasonNumber,
    createClientSideCollectionSelector('episodes'),
    createSeriesSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (seasonNumber, episodes, series, commands, dimensions) => {
      const isSearching = isCommandExecuting(findCommand(commands, {
        name: commandNames.SEASON_SEARCH,
        seriesId: series.id,
        seasonNumber
      }));

      const episodesInSeason = episodes.items.filter((episode) => episode.seasonNumber === seasonNumber);

      return {
        items: episodesInSeason,
        columns: episodes.columns,
        sortKey: episodes.sortKey,
        sortDirection: episodes.sortDirection,
        isSearching,
        seriesMonitored: series.monitored,
        path: series.path,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

const mapDispatchToProps = {
  toggleSeasonMonitored,
  toggleEpisodesMonitored,
  setEpisodesTableOption,
  setEpisodesSort,
  executeCommand
};

class SeriesDetailsSeasonConnector extends Component {

  //
  // Listeners

  onTableOptionChange = (payload) => {
    this.props.setEpisodesTableOption(payload);
  };

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
  };

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
  };

  onMonitorEpisodePress = (episodeIds, monitored) => {
    this.props.toggleEpisodesMonitored({
      episodeIds,
      monitored
    });
  };

  onSortPress = (sortKey, sortDirection) => {
    this.props.setEpisodesSort({
      sortKey,
      sortDirection
    });
  };

  //
  // Render

  render() {
    return (
      <SeriesDetailsSeason
        {...this.props}
        onTableOptionChange={this.onTableOptionChange}
        onSortPress={this.onSortPress}
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
  setEpisodesSort: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesDetailsSeasonConnector);
