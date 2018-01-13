import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { fetchEpisodes, clearEpisodes } from 'Store/Actions/episodeActions';
import { fetchEpisodeFiles, clearEpisodeFiles } from 'Store/Actions/episodeFileActions';
import { toggleSeriesMonitored } from 'Store/Actions/seriesActions';
import { fetchQueueDetails, clearQueueDetails } from 'Store/Actions/queueActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import SeriesDetails from './SeriesDetails';

function createMapStateToProps() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    (state) => state.episodes,
    (state) => state.episodeFiles,
    createAllSeriesSelector(),
    createCommandsSelector(),
    (titleSlug, episodes, episodeFiles, allSeries, commands) => {
      const sortedSeries = _.orderBy(allSeries, 'sortTitle');
      const seriesIndex = _.findIndex(sortedSeries, { titleSlug });
      const series = sortedSeries[seriesIndex];

      if (!series) {
        return {};
      }

      const previousSeries = sortedSeries[seriesIndex - 1] || _.last(sortedSeries);
      const nextSeries = sortedSeries[seriesIndex + 1] || _.first(sortedSeries);
      const isSeriesRefreshing = !!findCommand(commands, { name: commandNames.REFRESH_SERIES, seriesId: series.id });
      const allSeriesRefreshing = _.some(commands, (command) => command.name === commandNames.REFRESH_SERIES && !command.body.seriesId);
      const isRefreshing = isSeriesRefreshing || allSeriesRefreshing;
      const isSearching = !!findCommand(commands, { name: commandNames.SERIES_SEARCH, seriesId: series.id });
      const isRenamingFiles = !!findCommand(commands, { name: commandNames.RENAME_FILES, seriesId: series.id });
      const isRenamingSeriesCommand = findCommand(commands, { name: commandNames.RENAME_SERIES });
      const isRenamingSeries = !!(isRenamingSeriesCommand && isRenamingSeriesCommand.body.seriesId.indexOf(series.id) > -1);

      const isFetching = episodes.isFetching || episodeFiles.isFetching;
      const isPopulated = episodes.isPopulated && episodeFiles.isPopulated;
      const episodesError = episodes.error;
      const episodeFilesError = episodeFiles.error;
      const alternateTitles = _.reduce(series.alternateTitles, (acc, alternateTitle) => {
        if ((alternateTitle.seasonNumber === -1 || alternateTitle.seasonNumber === undefined) &&
            (alternateTitle.sceneSeasonNumber === -1 || alternateTitle.sceneSeasonNumber === undefined)) {
          acc.push(alternateTitle.title);
        }

        return acc;
      }, []);

      const hasMonitoredEpisodes = episodes.items.some((e) => e.monitored);

      return {
        ...series,
        alternateTitles,
        isSeriesRefreshing,
        allSeriesRefreshing,
        isRefreshing,
        isSearching,
        isRenamingFiles,
        isRenamingSeries,
        isFetching,
        isPopulated,
        episodesError,
        episodeFilesError,
        hasMonitoredEpisodes,
        previousSeries,
        nextSeries
      };
    }
  );
}

const mapDispatchToProps = {
  fetchEpisodes,
  clearEpisodes,
  fetchEpisodeFiles,
  clearEpisodeFiles,
  toggleSeriesMonitored,
  fetchQueueDetails,
  clearQueueDetails,
  executeCommand
};

class SeriesDetailsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.populate);
    this.populate();
  }

  componentDidUpdate(prevProps) {
    const {
      id,
      isSeriesRefreshing,
      allSeriesRefreshing,
      isRenamingFiles,
      isRenamingSeries
    } = this.props;

    if (
      (prevProps.isSeriesRefreshing && !isSeriesRefreshing) ||
      (prevProps.allSeriesRefreshing && !allSeriesRefreshing) ||
      (prevProps.isRenamingFiles && !isRenamingFiles) ||
      (prevProps.isRenamingSeries && !isRenamingSeries)
    ) {
      this.populate();
    }

    // If the id has changed we need to clear the episodes/episode
    // files and fetch from the server.

    if (prevProps.id !== id) {
      this.unpopulate();
      this.populate();
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.populate);
    this.unpopulate();
  }

  //
  // Control

  populate = () => {
    const seriesId = this.props.id;

    this.props.fetchEpisodes({ seriesId });
    this.props.fetchEpisodeFiles({ seriesId });
    this.props.fetchQueueDetails({ seriesId });
  }

  unpopulate = () => {
    this.props.clearEpisodes();
    this.props.clearEpisodeFiles();
    this.props.clearQueueDetails();
  }

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleSeriesMonitored({
      seriesId: this.props.id,
      monitored
    });
  }

  onRefreshPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_SERIES,
      seriesId: this.props.id
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.SERIES_SEARCH,
      seriesId: this.props.id
    });
  }

  //
  // Render

  render() {
    return (
      <SeriesDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

SeriesDetailsConnector.propTypes = {
  id: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  isSeriesRefreshing: PropTypes.bool.isRequired,
  allSeriesRefreshing: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isRenamingFiles: PropTypes.bool.isRequired,
  isRenamingSeries: PropTypes.bool.isRequired,
  fetchEpisodes: PropTypes.func.isRequired,
  clearEpisodes: PropTypes.func.isRequired,
  fetchEpisodeFiles: PropTypes.func.isRequired,
  clearEpisodeFiles: PropTypes.func.isRequired,
  toggleSeriesMonitored: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesDetailsConnector);
