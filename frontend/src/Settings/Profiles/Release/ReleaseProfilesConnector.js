import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteReleaseProfile, fetchIndexers, fetchReleaseProfiles } from 'Store/Actions/settingsActions';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import ReleaseProfiles from './ReleaseProfiles';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.releaseProfiles,
    (state) => state.settings.indexers,
    createTagsSelector(),
    (releaseProfiles, indexers, tagList) => {
      return {
        ...releaseProfiles,
        tagList,
        isIndexersPopulated: indexers.isPopulated,
        indexerList: indexers.items
      };
    }
  );
}

const mapDispatchToProps = {
  fetchIndexers,
  fetchReleaseProfiles,
  deleteReleaseProfile
};

class ReleaseProfilesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchReleaseProfiles();
    if (!this.props.isIndexersPopulated) {
      this.props.fetchIndexers();
    }
  }

  //
  // Listeners

  onConfirmDeleteReleaseProfile = (id) => {
    this.props.deleteReleaseProfile({ id });
  };

  //
  // Render

  render() {
    return (
      <ReleaseProfiles
        {...this.props}
        onConfirmDeleteReleaseProfile={this.onConfirmDeleteReleaseProfile}
      />
    );
  }
}

ReleaseProfilesConnector.propTypes = {
  isIndexersPopulated: PropTypes.bool.isRequired,
  fetchReleaseProfiles: PropTypes.func.isRequired,
  deleteReleaseProfile: PropTypes.func.isRequired,
  fetchIndexers: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ReleaseProfilesConnector);
