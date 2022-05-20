import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withCurrentPage from 'Components/withCurrentPage';
import { executeCommand } from 'Store/Actions/commandActions';
import * as systemActions from 'Store/Actions/systemActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import LogsTable from './LogsTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.logs,
    createCommandExecutingSelector(commandNames.CLEAR_LOGS),
    (logs, clearLogExecuting) => {
      return {
        clearLogExecuting,
        ...logs
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand,
  ...systemActions
};

class LogsTableConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      useCurrentPage,
      fetchLogs,
      gotoLogsFirstPage
    } = this.props;

    if (useCurrentPage) {
      fetchLogs();
    } else {
      gotoLogsFirstPage();
    }
  }

  componentDidUpdate(prevProps) {
    if (prevProps.clearLogExecuting && !this.props.clearLogExecuting) {
      this.props.gotoLogsFirstPage();
    }
  }

  //
  // Listeners

  onFirstPagePress = () => {
    this.props.gotoLogsFirstPage();
  };

  onPreviousPagePress = () => {
    this.props.gotoLogsPreviousPage();
  };

  onNextPagePress = () => {
    this.props.gotoLogsNextPage();
  };

  onLastPagePress = () => {
    this.props.gotoLogsLastPage();
  };

  onPageSelect = (page) => {
    this.props.gotoLogsPage({ page });
  };

  onSortPress = (sortKey) => {
    this.props.setLogsSort({ sortKey });
  };

  onFilterSelect = (selectedFilterKey) => {
    this.props.setLogsFilter({ selectedFilterKey });
  };

  onTableOptionChange = (payload) => {
    this.props.setLogsTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoLogsFirstPage();
    }
  };

  onRefreshPress = () => {
    this.props.gotoLogsFirstPage();
  };

  onClearLogsPress = () => {
    this.props.executeCommand({
      name: commandNames.CLEAR_LOGS,
      commandFinished: this.onCommandFinished
    });
  };

  onCommandFinished = () => {
    this.props.gotoLogsFirstPage();
  };

  //
  // Render

  render() {
    return (
      <LogsTable
        onFirstPagePress={this.onFirstPagePress}
        onPreviousPagePress={this.onPreviousPagePress}
        onNextPagePress={this.onNextPagePress}
        onLastPagePress={this.onLastPagePress}
        onPageSelect={this.onPageSelect}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onTableOptionChange={this.onTableOptionChange}
        onRefreshPress={this.onRefreshPress}
        onClearLogsPress={this.onClearLogsPress}
        {...this.props}
      />
    );
  }
}

LogsTableConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  clearLogExecuting: PropTypes.bool.isRequired,
  fetchLogs: PropTypes.func.isRequired,
  gotoLogsFirstPage: PropTypes.func.isRequired,
  gotoLogsPreviousPage: PropTypes.func.isRequired,
  gotoLogsNextPage: PropTypes.func.isRequired,
  gotoLogsLastPage: PropTypes.func.isRequired,
  gotoLogsPage: PropTypes.func.isRequired,
  setLogsSort: PropTypes.func.isRequired,
  setLogsFilter: PropTypes.func.isRequired,
  setLogsTableOption: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, mapDispatchToProps)(LogsTableConnector)
);
