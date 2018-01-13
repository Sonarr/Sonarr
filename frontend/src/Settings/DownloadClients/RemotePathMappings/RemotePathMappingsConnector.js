import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchRemotePathMappings, deleteRemotePathMapping } from 'Store/Actions/settingsActions';
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
  fetchRemotePathMappings,
  deleteRemotePathMapping
};

class RemotePathMappingsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchRemotePathMappings();
  }

  //
  // Listeners

  onConfirmDeleteRemotePathMapping = (id) => {
    this.props.deleteRemotePathMapping({ id });
  }

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
  fetchRemotePathMappings: PropTypes.func.isRequired,
  deleteRemotePathMapping: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(RemotePathMappingsConnector);
