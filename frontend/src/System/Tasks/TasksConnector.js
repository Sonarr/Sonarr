import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchTasks } from 'Store/Actions/systemActions';
import Tasks from './Tasks';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.tasks,
    (tasks) => {
      return tasks;
    }
  );
}

const mapDispatchToProps = {
  fetchTasks
};

class TasksConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchTasks();
  }

  //
  // Render

  render() {
    return (
      <Tasks
        {...this.props}
      />
    );
  }
}

TasksConnector.propTypes = {
  fetchTasks: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(TasksConnector);
