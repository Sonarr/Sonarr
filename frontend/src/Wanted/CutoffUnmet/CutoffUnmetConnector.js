import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import withCurrentPage from 'Components/withCurrentPage';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import * as wantedActions from 'Store/Actions/wantedActions';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchQueueDetails, clearQueueDetails } from 'Store/Actions/queueActions';
import { fetchEpisodeFiles, clearEpisodeFiles } from 'Store/Actions/episodeFileActions';
import * as commandNames from 'Commands/commandNames';
import CutoffUnmet from './CutoffUnmet';

function createMapStateToProps() {
  return createSelector(
    (state) => state.wanted.cutoffUnmet,
    createCommandExecutingSelector(commandNames.CUTOFF_UNMET_EPISODE_SEARCH),
    (cutoffUnmet, isSearchingForCutoffUnmetEpisodes) => {
      return {
        isSearchingForCutoffUnmetEpisodes,
        isSaving: cutoffUnmet.items.filter((m) => m.isSaving).length > 1,
        ...cutoffUnmet
      };
    }
  );
}

const mapDispatchToProps = {
  ...wantedActions,
  executeCommand,
  fetchQueueDetails,
  clearQueueDetails,
  fetchEpisodeFiles,
  clearEpisodeFiles
};

class CutoffUnmetConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchCutoffUnmet,
      gotoCutoffUnmetFirstPage
    } = this.props;

    registerPagePopulator(this.repopulate, ['episodeFileUpdated']);

    if (useCurrentPage) {
      fetchCutoffUnmet();
    } else {
      gotoCutoffUnmetFirstPage();
    }
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      const episodeIds = selectUniqueIds(this.props.items, 'id');
      const episodeFileIds = selectUniqueIds(this.props.items, 'episodeFileId');

      this.props.fetchQueueDetails({ episodeIds });

      if (episodeFileIds.length) {
        this.props.fetchEpisodeFiles({ episodeFileIds });
      }
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
    this.props.clearCutoffUnmet();
    this.props.clearQueueDetails();
    this.props.clearEpisodeFiles();
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchCutoffUnmet();
  }
  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoCutoffUnmetFirstPage();
  }

  onPreviousPagePress = () => {
    this.props.gotoCutoffUnmetPreviousPage();
  }

  onNextPagePress = () => {
    this.props.gotoCutoffUnmetNextPage();
  }

  onLastPagePress = () => {
    this.props.gotoCutoffUnmetLastPage();
  }

  onPageSelect = (page) => {
    this.props.gotoCutoffUnmetPage({ page });
  }

  onSortPress = (sortKey) => {
    this.props.setCutoffUnmetSort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.setCutoffUnmetFilter({ selectedFilterKey });
  }

  onTableOptionChange = (payload) => {
    this.props.setCutoffUnmetTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoCutoffUnmetFirstPage();
    }
  }

  onSearchSelectedPress = (selected) => {
    this.props.executeCommand({
      name: commandNames.EPISODE_SEARCH,
      episodeIds: selected
    });
  }

  onSearchAllCutoffUnmetPress = (monitored) => {
    this.props.executeCommand({
      name: commandNames.CUTOFF_UNMET_EPISODE_SEARCH,
      monitored
    });
  }

  //
  // Render

  render() {
    return (
      <CutoffUnmet
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onTableOptionChange={this.onTableOptionChange}
        onSearchSelectedPress={this.onSearchSelectedPress}
        onToggleSelectedPress={this.onToggleSelectedPress}
        onSearchAllCutoffUnmetPress={this.onSearchAllCutoffUnmetPress}
        {...this.props}
      />
    );
  }
}

CutoffUnmetConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchCutoffUnmet: PropTypes.func.isRequired,
  gotoCutoffUnmetFirstPage: PropTypes.func.isRequired,
  gotoCutoffUnmetPreviousPage: PropTypes.func.isRequired,
  gotoCutoffUnmetNextPage: PropTypes.func.isRequired,
  gotoCutoffUnmetLastPage: PropTypes.func.isRequired,
  gotoCutoffUnmetPage: PropTypes.func.isRequired,
  setCutoffUnmetSort: PropTypes.func.isRequired,
  setCutoffUnmetFilter: PropTypes.func.isRequired,
  setCutoffUnmetTableOption: PropTypes.func.isRequired,
  clearCutoffUnmet: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired,
  fetchEpisodeFiles: PropTypes.func.isRequired,
  clearEpisodeFiles: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, mapDispatchToProps)(CutoffUnmetConnector)
);
