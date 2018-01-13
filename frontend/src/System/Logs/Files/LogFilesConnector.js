import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import combinePath from 'Utilities/String/combinePath';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchLogFiles } from 'Store/Actions/systemActions';
import * as commandNames from 'Commands/commandNames';
import LogFiles from './LogFiles';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.logFiles,
    (state) => state.system.status.item,
    createCommandsSelector(),
    (logFiles, status, commands) => {
      const {
        isFetching,
        items
      } = logFiles;

      const {
        appData,
        isWindows
      } = status;

      const deleteFilesExecuting = _.some(commands, { name: commandNames.DELETE_LOG_FILES });

      return {
        isFetching,
        items,
        deleteFilesExecuting,
        currentLogView: 'Log Files',
        location: combinePath(isWindows, appData, ['logs'])
      };
    }
  );
}

const mapDispatchToProps = {
  fetchLogFiles,
  executeCommand
};

class LogFilesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchLogFiles();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.deleteFilesExecuting && !this.props.deleteFilesExecuting) {
      this.props.fetchLogFiles();
    }
  }

  //
  // Listeners

  onRefreshPress = () => {
    this.props.fetchLogFiles();
  }

  onDeleteFilesPress = () => {
    this.props.executeCommand({ name: commandNames.DELETE_LOG_FILES });
  }

  //
  // Render

  render() {
    return (
      <LogFiles
        onRefreshPress={this.onRefreshPress}
        onDeleteFilesPress={this.onDeleteFilesPress}
        {...this.props}
      />
    );
  }
}

LogFilesConnector.propTypes = {
  deleteFilesExecuting: PropTypes.bool.isRequired,
  fetchLogFiles: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(LogFilesConnector);
