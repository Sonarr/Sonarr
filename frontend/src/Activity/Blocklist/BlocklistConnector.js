import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withCurrentPage from 'Components/withCurrentPage';
import * as blocklistActions from 'Store/Actions/blocklistActions';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import Blocklist from './Blocklist';

function createMapStateToProps() {
  return createSelector(
    (state) => state.blocklist,
    createCommandExecutingSelector(commandNames.CLEAR_BLOCKLIST),
    (blocklist, isClearingBlocklistExecuting) => {
      return {
        isClearingBlocklistExecuting,
        ...blocklist
      };
    }
  );
}

const mapDispatchToProps = {
  ...blocklistActions,
  executeCommand
};

class BlocklistConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchBlocklist,
      gotoBlocklistFirstPage
    } = this.props;

    registerPagePopulator(this.repopulate);

    if (useCurrentPage) {
      fetchBlocklist();
    } else {
      gotoBlocklistFirstPage();
    }
  }

  componentDidUpdate(prevProps) {
    if (prevProps.isClearingBlocklistExecuting && !this.props.isClearingBlocklistExecuting) {
      this.props.gotoBlocklistFirstPage();
    }
  }

  componentWillUnmount() {
    this.props.clearBlocklist();
    unregisterPagePopulator(this.repopulate);
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchBlocklist();
  };
  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoBlocklistFirstPage();
  };

  onPreviousPagePress = () => {
    this.props.gotoBlocklistPreviousPage();
  };

  onNextPagePress = () => {
    this.props.gotoBlocklistNextPage();
  };

  onLastPagePress = () => {
    this.props.gotoBlocklistLastPage();
  };

  onPageSelect = (page) => {
    this.props.gotoBlocklistPage({ page });
  };

  onRemoveSelected = (ids) => {
    this.props.removeBlocklistItems({ ids });
  };

  onSortPress = (sortKey) => {
    this.props.setBlocklistSort({ sortKey });
  };

  onClearBlocklistPress = () => {
    this.props.executeCommand({ name: commandNames.CLEAR_BLOCKLIST });
  };

  onTableOptionChange = (payload) => {
    this.props.setBlocklistTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoBlocklistFirstPage();
    }
  };

  //
  // Render

  render() {
    return (
      <Blocklist
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onRemoveSelected={this.onRemoveSelected}
        onSortPress={this.onSortPress}
        onTableOptionChange={this.onTableOptionChange}
        onClearBlocklistPress={this.onClearBlocklistPress}
        {...this.props}
      />
    );
  }
}

BlocklistConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  isClearingBlocklistExecuting: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchBlocklist: PropTypes.func.isRequired,
  gotoBlocklistFirstPage: PropTypes.func.isRequired,
  gotoBlocklistPreviousPage: PropTypes.func.isRequired,
  gotoBlocklistNextPage: PropTypes.func.isRequired,
  gotoBlocklistLastPage: PropTypes.func.isRequired,
  gotoBlocklistPage: PropTypes.func.isRequired,
  removeBlocklistItems: PropTypes.func.isRequired,
  setBlocklistSort: PropTypes.func.isRequired,
  setBlocklistTableOption: PropTypes.func.isRequired,
  clearBlocklist: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, mapDispatchToProps)(BlocklistConnector)
);
