import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchStatus } from 'Store/Actions/systemActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import About from './About';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.status,
    createUISettingsSelector(),
    (status, uiSettings) => {
      return {
        ...status.item,
        timeFormat: uiSettings.timeFormat,
        longDateFormat: uiSettings.longDateFormat
      };
    }
  );
}

const mapDispatchToProps = {
  fetchStatus
};

class AboutConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchStatus();
  }

  //
  // Render

  render() {
    return (
      <About
        {...this.props}
      />
    );
  }
}

AboutConnector.propTypes = {
  fetchStatus: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AboutConnector);
