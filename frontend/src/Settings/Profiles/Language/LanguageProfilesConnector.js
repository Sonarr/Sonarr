import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cloneLanguageProfile, deleteLanguageProfile, fetchLanguageProfiles } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import sortByName from 'Utilities/Array/sortByName';
import LanguageProfiles from './LanguageProfiles';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSortedSectionSelector('settings.languageProfiles', sortByName),
    (advancedSettings, languageProfiles) => {
      return {
        advancedSettings,
        ...languageProfiles
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchLanguageProfiles: fetchLanguageProfiles,
  dispatchDeleteLanguageProfile: deleteLanguageProfile,
  dispatchCloneLanguageProfile: cloneLanguageProfile
};

class LanguageProfilesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchLanguageProfiles();
  }

  //
  // Listeners

  onConfirmDeleteLanguageProfile = (id) => {
    this.props.dispatchDeleteLanguageProfile({ id });
  };

  onCloneLanguageProfilePress = (id) => {
    this.props.dispatchCloneLanguageProfile({ id });
  };

  //
  // Render

  render() {
    return (
      <LanguageProfiles
        onConfirmDeleteLanguageProfile={this.onConfirmDeleteLanguageProfile}
        onCloneLanguageProfilePress={this.onCloneLanguageProfilePress}
        {...this.props}
      />
    );
  }
}

LanguageProfilesConnector.propTypes = {
  dispatchFetchLanguageProfiles: PropTypes.func.isRequired,
  dispatchDeleteLanguageProfile: PropTypes.func.isRequired,
  dispatchCloneLanguageProfile: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(LanguageProfilesConnector);
