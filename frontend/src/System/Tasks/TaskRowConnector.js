import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { findCommand } from 'Utilities/Command';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchTask } from 'Store/Actions/systemActions';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import TaskRow from './TaskRow';

function createMapStateToProps() {
  return createSelector(
    (state, { taskName }) => taskName,
    createCommandsSelector(),
    createUISettingsSelector(),
    (taskName, commands, uiSettings) => {
      const isExecuting = !!findCommand(commands, { name: taskName });

      return {
        isExecuting,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        longDateFormat: uiSettings.longDateFormat,
        timeFormat: uiSettings.timeFormat
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  const taskName = props.taskName;

  return {
    dispatchFetchTask() {
      dispatch(fetchTask({
        id: props.id
      }));
    },

    onExecutePress() {
      dispatch(executeCommand({
        name: taskName
      }));
    }
  };
}

class TaskRowConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps) {
    const {
      isExecuting,
      dispatchFetchTask
    } = this.props;

    if (!isExecuting && prevProps.isExecuting) {
      // Give the host a moment to update after the command completes
      setTimeout(() => {
        dispatchFetchTask();
      }, 1000);
    }
  }

  //
  // Render

  render() {
    const {
      dispatchFetchTask,
      ...otherProps
    } = this.props;

    return (
      <TaskRow
        {...otherProps}
      />
    );
  }
}

TaskRowConnector.propTypes = {
  id: PropTypes.number.isRequired,
  isExecuting: PropTypes.bool.isRequired,
  dispatchFetchTask: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(TaskRowConnector);
