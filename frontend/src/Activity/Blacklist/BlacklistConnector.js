import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import * as blacklistActions from 'Store/Actions/blacklistActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import Blacklist from './Blacklist';

function createMapStateToProps() {
  return createSelector(
    (state) => state.blacklist,
    createCommandsSelector(),
    (blacklist, commands) => {
      const isClearingBlacklistExecuting = _.some(commands, { name: commandNames.CLEAR_BLACKLIST });

      return {
        isClearingBlacklistExecuting,
        ...blacklist
      };
    }
  );
}

const mapDispatchToProps = {
  ...blacklistActions,
  executeCommand
};

class BlacklistConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.repopulate);
    this.props.gotoBlacklistFirstPage();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.isClearingBlacklistExecuting && !this.props.isClearingBlacklistExecuting) {
      this.props.gotoBlacklistFirstPage();
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchBlacklist();
  }
  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoBlacklistFirstPage();
  }

  onPreviousPagePress = () => {
    this.props.gotoBlacklistPreviousPage();
  }

  onNextPagePress = () => {
    this.props.gotoBlacklistNextPage();
  }

  onLastPagePress = () => {
    this.props.gotoBlacklistLastPage();
  }

  onPageSelect = (page) => {
    this.props.gotoBlacklistPage({ page });
  }

  onSortPress = (sortKey) => {
    this.props.setBlacklistSort({ sortKey });
  }

  onTableOptionChange = (payload) => {
    this.props.setBlacklistTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoBlacklistFirstPage();
    }
  }

  onClearBlacklistPress = () => {
    this.props.executeCommand({ name: commandNames.CLEAR_BLACKLIST });
  }

  //
  // Render

  render() {
    return (
      <Blacklist
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onSortPress={this.onSortPress}
        onTableOptionChange={this.onTableOptionChange}
        onClearBlacklistPress={this.onClearBlacklistPress}
        {...this.props}
      />
    );
  }
}

BlacklistConnector.propTypes = {
  isClearingBlacklistExecuting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchBlacklist: PropTypes.func.isRequired,
  gotoBlacklistFirstPage: PropTypes.func.isRequired,
  gotoBlacklistPreviousPage: PropTypes.func.isRequired,
  gotoBlacklistNextPage: PropTypes.func.isRequired,
  gotoBlacklistLastPage: PropTypes.func.isRequired,
  gotoBlacklistPage: PropTypes.func.isRequired,
  setBlacklistSort: PropTypes.func.isRequired,
  setBlacklistTableOption: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(BlacklistConnector);
