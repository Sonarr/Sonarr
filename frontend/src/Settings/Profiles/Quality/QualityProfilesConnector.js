import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchQualityProfiles, deleteQualityProfile } from 'Store/Actions/settingsActions';
import QualityProfiles from './QualityProfiles';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.qualityProfiles,
    (qualityProfiles) => {
      return {
        ...qualityProfiles
      };
    }
  );
}

const mapDispatchToProps = {
  fetchQualityProfiles,
  deleteQualityProfile
};

class QualityProfilesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchQualityProfiles();
  }

  //
  // Listeners

  onConfirmDeleteQualityProfile = (id) => {
    this.props.deleteQualityProfile({ id });
  }

  //
  // Render

  render() {
    return (
      <QualityProfiles
        onConfirmDeleteQualityProfile={this.onConfirmDeleteQualityProfile}
        {...this.props}
      />
    );
  }
}

QualityProfilesConnector.propTypes = {
  fetchQualityProfiles: PropTypes.func.isRequired,
  deleteQualityProfile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(QualityProfilesConnector);
