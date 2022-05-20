import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { clearEpisodes, fetchEpisodes } from 'Store/Actions/episodeActions';
import { clearEpisodeFiles, fetchEpisodeFiles } from 'Store/Actions/episodeFileActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import { toggleSeriesMonitored } from 'Store/Actions/seriesActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import filterAlternateTitles from 'Utilities/Series/filterAlternateTitles';
import SeriesDetails from './SeriesDetails';

const selectEpisodes = createSelector(
  (state) => state.episodes,
  (episodes) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = episodes;

    const hasEpisodes = !!items.length;
    const hasMonitoredEpisodes = items.some((e) => e.monitored);

    return {
      isEpisodesFetching: isFetching,
      isEpisodesPopulated: isPopulated,
      episodesError: error,
      hasEpisodes,
      hasMonitoredEpisodes
    };
  }
);

const selectEpisodeFiles = createSelector(
  (state) => state.episodeFiles,
  (episodeFiles) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = episodeFiles;

    const hasEpisodeFiles = !!items.length;

    return {
      isEpisodeFilesFetching: isFetching,
      isEpisodeFilesPopulated: isPopulated,
      episodeFilesError: error,
      hasEpisodeFiles
    };
  }
);

function createMapStateToProps() {
  return createSelector(
    (state, { titleSlug }) => titleSlug,
    selectEpisodes,
    selectEpisodeFiles,
    createAllSeriesSelector(),
    createCommandsSelector(),
    (titleSlug, episodes, episodeFiles, allSeries, commands) => {
      const sortedSeries = _.orderBy(allSeries, 'sortTitle');
      const seriesIndex = _.findIndex(sortedSeries, { titleSlug });
      const series = sortedSeries[seriesIndex];

      if (!series) {
        return {};
      }

      const {
        isEpisodesFetching,
        isEpisodesPopulated,
        episodesError,
        hasEpisodes,
        hasMonitoredEpisodes
      } = episodes;

      const {
        isEpisodeFilesFetching,
        isEpisodeFilesPopulated,
        episodeFilesError,
        hasEpisodeFiles
      } = episodeFiles;

      const previousSeries = sortedSeries[seriesIndex - 1] || _.last(sortedSeries);
      const nextSeries = sortedSeries[seriesIndex + 1] || _.first(sortedSeries);
      const isSeriesRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_SERIES, seriesId: series.id }));
      const seriesRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_SERIES });
      const allSeriesRefreshing = (
        isCommandExecuting(seriesRefreshingCommand) &&
        !seriesRefreshingCommand.body.seriesId
      );
      const isRefreshing = isSeriesRefreshing || allSeriesRefreshing;
      const isSearching = isCommandExecuting(findCommand(commands, { name: commandNames.SERIES_SEARCH, seriesId: series.id }));
      const isRenamingFiles = isCommandExecuting(findCommand(commands, { name: commandNames.RENAME_FILES, seriesId: series.id }));
      const isRenamingSeriesCommand = findCommand(commands, { name: commandNames.RENAME_SERIES });
      const isRenamingSeries = (
        isCommandExecuting(isRenamingSeriesCommand) &&
        isRenamingSeriesCommand.body.seriesIds.indexOf(series.id) > -1
      );

      const isFetching = isEpisodesFetching || isEpisodeFilesFetching;
      const isPopulated = isEpisodesPopulated && isEpisodeFilesPopulated;
      const alternateTitles = filterAlternateTitles(series.alternateTitles, series.title, series.useSceneNumbering);

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
        hasEpisodes,
        hasMonitoredEpisodes,
        hasEpisodeFiles,
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
  };

  unpopulate = () => {
    this.props.clearEpisodes();
    this.props.clearEpisodeFiles();
    this.props.clearQueueDetails();
  };

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleSeriesMonitored({
      seriesId: this.props.id,
      monitored
    });
  };

  onRefreshPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_SERIES,
      seriesId: this.props.id
    });
  };

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.SERIES_SEARCH,
      seriesId: this.props.id
    });
  };

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
