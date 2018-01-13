import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import getFilterValue from 'Utilities/Filter/getFilterValue';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import * as wantedActions from 'Store/Actions/wantedActions';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchQueueDetails, clearQueueDetails } from 'Store/Actions/queueActions';
import * as commandNames from 'Commands/commandNames';
import Missing from './Missing';

function createMapStateToProps() {
  return createSelector(
    (state) => state.wanted.missing,
    createCommandsSelector(),
    (missing, commands) => {
      const isSearchingForEpisodes = _.some(commands, { name: commandNames.EPISODE_SEARCH });
      const isSearchingForMissingEpisodes = _.some(commands, { name: commandNames.MISSING_EPISODE_SEARCH });

      return {
        isSearchingForEpisodes,
        isSearchingForMissingEpisodes,
        isSaving: _.some(missing.items, { isSaving: true }),
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
    registerPagePopulator(this.repopulate);
    this.props.gotoMissingFirstPage();
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
  }

  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoMissingFirstPage();
  }

  onPreviousPagePress = () => {
    this.props.gotoMissingPreviousPage();
  }

  onNextPagePress = () => {
    this.props.gotoMissingNextPage();
  }

  onLastPagePress = () => {
    this.props.gotoMissingLastPage();
  }

  onPageSelect = (page) => {
    this.props.gotoMissingPage({ page });
  }

  onSortPress = (sortKey) => {
    this.props.setMissingSort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.setMissingFilter({ selectedFilterKey });
  }

  onTableOptionChange = (payload) => {
    this.props.setMissingTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoMissingFirstPage();
    }
  }

  onSearchSelectedPress = (selected) => {
    this.props.executeCommand({
      name: commandNames.EPISODE_SEARCH,
      episodeIds: selected
    });
  }

  onToggleSelectedPress = (selected) => {
    const {
      filters
    } = this.props;

    const monitored = getFilterValue(filters, 'monitored');

    this.props.batchToggleMissingEpisodes({
      episodeIds: selected,
      monitored: monitored == null || !monitored
    });
  }

  onSearchAllMissingPress = () => {
    this.props.executeCommand({
      name: commandNames.MISSING_EPISODE_SEARCH
    });
  }

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
        onToggleSelectedPress={this.onToggleSelectedPress}
        onSearchAllMissingPress={this.onSearchAllMissingPress}
        {...this.props}
      />
    );
  }
}

MissingConnector.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
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
  batchToggleMissingEpisodes: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MissingConnector);
