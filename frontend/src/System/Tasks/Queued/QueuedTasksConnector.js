import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchCommands } from 'Store/Actions/commandActions';
import QueuedTasks from './QueuedTasks';

function createMapStateToProps() {
  return createSelector(
    (state) => state.commands,
    (commands) => {
      return commands;
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchCommands: fetchCommands
};

class QueuedTasksConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchCommands();
  }

  //
  // Render

  render() {
    return (
      <QueuedTasks
        {...this.props}
      />
    );
  }
}

QueuedTasksConnector.propTypes = {
  dispatchFetchCommands: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(QueuedTasksConnector);
