import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchLanguageProfiles, deleteLanguageProfile } from 'Store/Actions/settingsActions';
import LanguageProfiles from './LanguageProfiles';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    (state) => state.settings.languageProfiles,
    (advancedSettings, languageProfiles) => {
      return {
        advancedSettings,
        ...languageProfiles
      };
    }
  );
}

const mapDispatchToProps = {
  fetchLanguageProfiles,
  deleteLanguageProfile
};

class LanguageProfilesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchLanguageProfiles();
  }

  //
  // Listeners

  onConfirmDeleteLanguageProfile = (id) => {
    this.props.deleteLanguageProfile({ id });
  }

  //
  // Render

  render() {
    return (
      <LanguageProfiles
        onConfirmDeleteLanguageProfile={this.onConfirmDeleteLanguageProfile}
        {...this.props}
      />
    );
  }
}

LanguageProfilesConnector.propTypes = {
  fetchLanguageProfiles: PropTypes.func.isRequired,
  deleteLanguageProfile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(LanguageProfilesConnector);
