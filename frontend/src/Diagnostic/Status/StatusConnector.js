// @ts-check
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchStatus } from 'Store/Actions/diagnosticActions';
import Status from './Status';

function createMapStateToProps() {
  return createSelector(
    (state) => state.diagnostic.status,
    (status) => {
      return {
        isStatusFetching: status.isFetching,
        status: status.item
      };
    }
  );
}

const mapDispatchToProps = {
  fetchStatus
};

class DiagnosticConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchStatus();
  }

  //
  // Listeners

  onRefreshPress = () => {
    this.props.fetchStatus();
  }

  //
  // Render

  render() {
    return (
      <Status
        onRefreshPress={this.onRefreshPress}
        {...this.props}
      />
    );
  }
}

DiagnosticConnector.propTypes = {
  status: PropTypes.object.isRequired,
  fetchStatus: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DiagnosticConnector);
