import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchDownloadClients, deleteDownloadClient } from 'Store/Actions/settingsActions';
import DownloadClients from './DownloadClients';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.downloadClients,
    (downloadClients) => {
      return {
        ...downloadClients
      };
    }
  );
}

const mapDispatchToProps = {
  fetchDownloadClients,
  deleteDownloadClient
};

class DownloadClientsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchDownloadClients();
  }

  //
  // Listeners

  onConfirmDeleteDownloadClient = (id) => {
    this.props.deleteDownloadClient({ id });
  }

  //
  // Render

  render() {
    return (
      <DownloadClients
        {...this.props}
        onConfirmDeleteDownloadClient={this.onConfirmDeleteDownloadClient}
      />
    );
  }
}

DownloadClientsConnector.propTypes = {
  fetchDownloadClients: PropTypes.func.isRequired,
  deleteDownloadClient: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DownloadClientsConnector);
