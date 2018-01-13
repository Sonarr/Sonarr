import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as systemActions from 'Store/Actions/systemActions';
import * as commandNames from 'Commands/commandNames';
import LogsTable from './LogsTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.logs,
    createCommandsSelector(),
    (logs, commands) => {
      const clearLogExecuting = _.some(commands, { name: commandNames.CLEAR_LOGS });

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
    this.props.fetchLogs();
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
  }

  onPreviousPagePress = () => {
    this.props.gotoLogsPreviousPage();
  }

  onNextPagePress = () => {
    this.props.gotoLogsNextPage();
  }

  onLastPagePress = () => {
    this.props.gotoLogsLastPage();
  }

  onPageSelect = (page) => {
    this.props.gotoLogsPage({ page });
  }

  onSortPress = (sortKey) => {
    this.props.setLogsSort({ sortKey });
  }

  onFilterSelect = (selectedFilterKey) => {
    this.props.setLogsFilter({ selectedFilterKey });
  }

  onTableOptionChange = (payload) => {
    this.props.setLogsTableOption(payload);

    if (payload.pageSize) {
      this.props.gotoLogsFirstPage();
    }
  }

  onRefreshPress = () => {
    this.props.gotoLogsFirstPage();
  }

  onClearLogsPress = () => {
    this.props.executeCommand({ name: commandNames.CLEAR_LOGS });
  }

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

export default connect(createMapStateToProps, mapDispatchToProps)(LogsTableConnector);
