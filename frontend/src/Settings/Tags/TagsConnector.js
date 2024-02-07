import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchDelayProfiles, fetchDownloadClients, fetchImportLists, fetchIndexers, fetchNotifications, fetchReleaseProfiles } from 'Store/Actions/settingsActions';
import { fetchTagDetails, fetchTags } from 'Store/Actions/tagActions';
import Tags from './Tags';

function createMapStateToProps() {
  return createSelector(
    (state) => state.tags,
    (tags) => {
      const isFetching = tags.isFetching || tags.details.isFetching;
      const error = tags.error || tags.details.error;
      const isPopulated = tags.isPopulated && tags.details.isPopulated;

      return {
        ...tags,
        isFetching,
        error,
        isPopulated
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchTags: fetchTags,
  dispatchFetchTagDetails: fetchTagDetails,
  dispatchFetchDelayProfiles: fetchDelayProfiles,
  dispatchFetchImportLists: fetchImportLists,
  dispatchFetchNotifications: fetchNotifications,
  dispatchFetchReleaseProfiles: fetchReleaseProfiles,
  dispatchFetchIndexers: fetchIndexers,
  dispatchFetchDownloadClients: fetchDownloadClients
};

class MetadatasConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchTags,
      dispatchFetchTagDetails,
      dispatchFetchDelayProfiles,
      dispatchFetchImportLists,
      dispatchFetchNotifications,
      dispatchFetchReleaseProfiles,
      dispatchFetchIndexers,
      dispatchFetchDownloadClients
    } = this.props;

    dispatchFetchTags();
    dispatchFetchTagDetails();
    dispatchFetchDelayProfiles();
    dispatchFetchImportLists();
    dispatchFetchNotifications();
    dispatchFetchReleaseProfiles();
    dispatchFetchIndexers();
    dispatchFetchDownloadClients();
  }

  //
  // Render

  render() {
    return (
      <Tags
        {...this.props}
      />
    );
  }
}

MetadatasConnector.propTypes = {
  dispatchFetchTags: PropTypes.func.isRequired,
  dispatchFetchTagDetails: PropTypes.func.isRequired,
  dispatchFetchDelayProfiles: PropTypes.func.isRequired,
  dispatchFetchImportLists: PropTypes.func.isRequired,
  dispatchFetchNotifications: PropTypes.func.isRequired,
  dispatchFetchReleaseProfiles: PropTypes.func.isRequired,
  dispatchFetchIndexers: PropTypes.func.isRequired,
  dispatchFetchDownloadClients: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadatasConnector);
