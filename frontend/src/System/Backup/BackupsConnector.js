import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { deleteBackup, fetchBackups } from 'Store/Actions/systemActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import Backups from './Backups';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.backups,
    (state) => state.system.status.item,
    createCommandExecutingSelector(commandNames.BACKUP),
    (backups, status, backupExecuting) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = backups;
      const { backupFolder } = status;

      return {
        isFetching,
        isPopulated,
        error,
        items,
        backupExecuting,
        location: backupFolder
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchBackups() {
      dispatch(fetchBackups());
    },

    onDeleteBackupPress(id) {
      dispatch(deleteBackup({ id }));
    },

    onBackupPress() {
      dispatch(executeCommand({
        name: commandNames.BACKUP
      }));
    }
  };
}

class BackupsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchBackups();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.backupExecuting && !this.props.backupExecuting) {
      this.props.dispatchFetchBackups();
    }
  }

  //
  // Render

  render() {
    return (
      <Backups
        {...this.props}
      />
    );
  }
}

BackupsConnector.propTypes = {
  backupExecuting: PropTypes.bool.isRequired,
  dispatchFetchBackups: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(BackupsConnector);
