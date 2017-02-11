import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchHealth } from 'Store/Actions/systemActions';
import Health from './Health';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.health,
    (health) => {
      const {
        isFetching,
        isPopulated,
        items
      } = health;

      return {
        isFetching,
        isPopulated,
        items
      };
    }
  );
}

const mapDispatchToProps = {
  fetchHealth
};

class HealthConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchHealth();
  }

  //
  // Render

  render() {
    return (
      <Health
        {...this.props}
      />
    );
  }
}

HealthConnector.propTypes = {
  fetchHealth: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(HealthConnector);
