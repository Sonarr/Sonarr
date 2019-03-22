import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchHealth } from 'Store/Actions/systemActions';
import { testAllDownloadClients, testAllIndexers } from 'Store/Actions/settingsActions';
import Health from './Health';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.health,
    (state) => state.settings.downloadClients.isTestingAll,
    (state) => state.settings.indexers.isTestingAll,
    (health, isTestingAllDownloadClients, isTestingAllIndexers) => {
      const {
        isFetching,
        isPopulated,
        items
      } = health;

      return {
        isFetching,
        isPopulated,
        items,
        isTestingAllDownloadClients,
        isTestingAllIndexers
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchHealth: fetchHealth,
  dispatchTestAllDownloadClients: testAllDownloadClients,
  dispatchTestAllIndexers: testAllIndexers
};

class HealthConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchHealth();
  }

  //
  // Render

  render() {
    const {
      dispatchFetchHealth,
      ...otherProps
    } = this.props;

    return (
      <Health
        {...otherProps}
      />
    );
  }
}

HealthConnector.propTypes = {
  dispatchFetchHealth: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(HealthConnector);
