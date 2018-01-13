import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchTagDetails } from 'Store/Actions/tagActions';
import { fetchDelayProfiles, fetchNotifications, fetchRestrictions } from 'Store/Actions/settingsActions';
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
  dispatchFetchTagDetails: fetchTagDetails,
  dispatchFetchDelayProfiles: fetchDelayProfiles,
  dispatchFetchNotifications: fetchNotifications,
  dispatchFetchRestrictions: fetchRestrictions
};

class MetadatasConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchTagDetails,
      dispatchFetchDelayProfiles,
      dispatchFetchNotifications,
      dispatchFetchRestrictions
    } = this.props;

    dispatchFetchTagDetails();
    dispatchFetchDelayProfiles();
    dispatchFetchNotifications();
    dispatchFetchRestrictions();
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
  dispatchFetchTagDetails: PropTypes.func.isRequired,
  dispatchFetchDelayProfiles: PropTypes.func.isRequired,
  dispatchFetchNotifications: PropTypes.func.isRequired,
  dispatchFetchRestrictions: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MetadatasConnector);
