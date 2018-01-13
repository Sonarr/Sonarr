import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchReleaseProfiles, deleteReleaseProfile } from 'Store/Actions/settingsActions';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import ReleaseProfiles from './ReleaseProfiles';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.releaseProfiles,
    createTagsSelector(),
    (releaseProfiles, tagList) => {
      return {
        ...releaseProfiles,
        tagList
      };
    }
  );
}

const mapDispatchToProps = {
  fetchReleaseProfiles,
  deleteReleaseProfile
};

class ReleaseProfilesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchReleaseProfiles();
  }

  //
  // Listeners

  onConfirmDeleteReleaseProfile = (id) => {
    this.props.deleteReleaseProfile({ id });
  }

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
  fetchReleaseProfiles: PropTypes.func.isRequired,
  deleteReleaseProfile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ReleaseProfilesConnector);
