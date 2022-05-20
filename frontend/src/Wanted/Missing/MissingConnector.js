import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withCurrentPage from 'Components/withCurrentPage';
import { executeCommand } from 'Store/Actions/commandActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import * as wantedActions from 'Store/Actions/wantedActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import Missing from './Missing';

function createMapStateToProps() {
  return createSelector(
    (state) => state.wanted.missing,
    createCommandExecutingSelector(commandNames.MISSING_EPISODE_SEARCH),
    (missing, isSearchingForMissingEpisodes) => {
      return {
        isSearchingForMissingEpisodes,
        isSaving: missing.items.filter((m) => m.isSaving).length > 1,
        ...missing
      };
    }
  );
}

const mapDispatchToProps = {
  ...wantedActions,
  executeCommand,
  fetchQueueDetails,
  clearQueueDetails
};

class MissingConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchMissing,
      gotoMissingFirstPage
    } = this.props;

    registerPagePopulator(this.repopulate, ['episodeFileUpdated']);

    if (useCurrentPage) {
      fetchMissing();
    } else {
      gotoMissingFirstPage();
    }
  }

  componentDidUpdate(prevProps) {
    if (hasDifferentItems(prevProps.items, this.props.items)) {
      const episodeIds = selectUniqueIds(this.props.items, 'id');
      this.props.fetchQueueDetails({ episodeIds });
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
    this.props.clearMissing();
    this.props.clearQueueDetails();
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchMissing();
  };

  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoMissingFirstPage();
  };

  onPreviousPagePress = () => {
    this.props.gotoMissingPreviousPage();
  };

  onNextPagePress = () => {
    this.props.gotoMissingNextPage();
  };

  onLastPagePress = () => {
    this.props.gotoMissingLastPage();
  };

  onPageSelect = (page) => {
    this.props.gotoMissingPage({ page });
  };

  onSortPress = (sortKey) => {
    this.props.setMissingSort({ sortKey });
  };

  onFilterSelect = (selectedFilterKey) => {
    this.props.setMissingFilter({ selectedFilterKey });
  };

  onTableOptionChange = (payload) => {
    this.props.setMissingTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoMissingFirstPage();
    }
  };

  onSearchSelectedPress = (selected) => {
    this.props.executeCommand({
      name: commandNames.EPISODE_SEARCH,
      episodeIds: selected
    });
  };

  onSearchAllMissingPress = (monitored) => {
    this.props.executeCommand({
      name: commandNames.MISSING_EPISODE_SEARCH,
      monitored
    });
  };

  //
  // Render

  render() {
    return (
      <Missing
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onTableOptionChange={this.onTableOptionChange}
        onSearchSelectedPress={this.onSearchSelectedPress}
        onSearchAllMissingPress={this.onSearchAllMissingPress}
        {...this.props}
      />
    );
  }
}

MissingConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchMissing: PropTypes.func.isRequired,
  gotoMissingFirstPage: PropTypes.func.isRequired,
  gotoMissingPreviousPage: PropTypes.func.isRequired,
  gotoMissingNextPage: PropTypes.func.isRequired,
  gotoMissingLastPage: PropTypes.func.isRequired,
  gotoMissingPage: PropTypes.func.isRequired,
  setMissingSort: PropTypes.func.isRequired,
  setMissingFilter: PropTypes.func.isRequired,
  setMissingTableOption: PropTypes.func.isRequired,
  clearMissing: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, mapDispatchToProps)(MissingConnector)
);
