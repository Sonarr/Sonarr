import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchQueueStatus } from 'Store/Actions/queueActions';
import PageSidebarStatus from 'Components/Page/Sidebar/PageSidebarStatus';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app,
    (state) => state.queue.status,
    (app, status) => {
      return {
        isConnected: app.isConnected,
        isReconnecting: app.isReconnecting,
        isPopulated: status.isPopulated,
        ...status.item
      };
    }
  );
}

const mapDispatchToProps = {
  fetchQueueStatus
};

class QueueStatusConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.isPopulated) {
      this.props.fetchQueueStatus();
    }
  }

  componentDidUpdate(prevProps) {
    if (this.props.isConnected && prevProps.isReconnecting) {
      this.props.fetchQueueStatus();
    }
  }

  //
  // Render

  render() {
    return (
      <PageSidebarStatus
        {...this.props}
      />
    );
  }
}

QueueStatusConnector.propTypes = {
  isConnected: PropTypes.bool.isRequired,
  isReconnecting: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  fetchQueueStatus: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(QueueStatusConnector);
