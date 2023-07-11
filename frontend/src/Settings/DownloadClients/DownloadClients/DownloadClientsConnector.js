import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteDownloadClient, fetchDownloadClients } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import sortByName from 'Utilities/Array/sortByName';
import DownloadClients from './DownloadClients';

function createMapStateToProps() {
  return createSelector(
    createSortedSectionSelector('settings.downloadClients', sortByName),
    createTagsSelector(),
    (downloadClients, tagList) => {
      return {
        ...downloadClients,
        tagList
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
  };

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
