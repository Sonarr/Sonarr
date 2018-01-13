import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchHealth } from 'Store/Actions/systemActions';
import PageSidebarStatus from 'Components/Page/Sidebar/PageSidebarStatus';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app,
    (state) => state.system.health,
    (app, health) => {
      const count = health.items.length;
      let errors = false;
      let warnings = false;

      health.items.forEach((item) => {
        if (item.type === 'error') {
          errors = true;
        }

        if (item.type === 'warning') {
          warnings = true;
        }
      });

      return {
        isConnected: app.isConnected,
        isReconnecting: app.isReconnecting,
        isPopulated: health.isPopulated,
        count,
        errors,
        warnings
      };
    }
  );
}

const mapDispatchToProps = {
  fetchHealth
};

class HealthStatusConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.isPopulated) {
      this.props.fetchHealth();
    }
  }

  componentDidUpdate(prevProps) {
    if (this.props.isConnected && prevProps.isReconnecting) {
      this.props.fetchHealth();
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

HealthStatusConnector.propTypes = {
  isConnected: PropTypes.bool.isRequired,
  isReconnecting: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  fetchHealth: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(HealthStatusConnector);
