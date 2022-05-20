import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteRemotePathMapping, fetchRemotePathMappings } from 'Store/Actions/settingsActions';
import RemotePathMappings from './RemotePathMappings';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.remotePathMappings,
    (remotePathMappings) => {
      return {
        ...remotePathMappings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchRemotePathMappings: fetchRemotePathMappings,
  dispatchDeleteRemotePathMapping: deleteRemotePathMapping
};

class RemotePathMappingsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchRemotePathMappings();
  }

  //
  // Listeners

  onConfirmDeleteRemotePathMapping = (id) => {
    this.props.dispatchDeleteRemotePathMapping({ id });
  };

  //
  // Render

  render() {
    return (
      <RemotePathMappings
        {...this.state}
        {...this.props}
        onConfirmDeleteRemotePathMapping={this.onConfirmDeleteRemotePathMapping}
      />
    );
  }
}

RemotePathMappingsConnector.propTypes = {
  dispatchFetchRemotePathMappings: PropTypes.func.isRequired,
  dispatchDeleteRemotePathMapping: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(RemotePathMappingsConnector);
