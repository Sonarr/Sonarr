import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import { fetchMediaManagementSettings, setMediaManagementSettingsValue, saveMediaManagementSettings, saveNamingSettings } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import MediaManagement from './MediaManagement';

const SECTION = 'mediaManagement';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    (state) => state.settings.naming,
    createSettingsSectionSelector(SECTION),
    createSystemStatusSelector(),
    (advancedSettings, namingSettings, sectionSettings, systemStatus) => {
      return {
        advancedSettings,
        ...sectionSettings,
        hasPendingChanges: !_.isEmpty(namingSettings.pendingChanges) || sectionSettings.hasPendingChanges,
        isMono: systemStatus.isMono
      };
    }
  );
}

const mapDispatchToProps = {
  fetchMediaManagementSettings,
  setMediaManagementSettingsValue,
  saveMediaManagementSettings,
  saveNamingSettings,
  clearPendingChanges
};

class MediaManagementConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchMediaManagementSettings();
  }

  componentWillUnmount() {
    this.props.clearPendingChanges({ section: SECTION });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setMediaManagementSettingsValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveMediaManagementSettings();
    this.props.saveNamingSettings();
  }

  //
  // Render

  render() {
    return (
      <MediaManagement
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        {...this.props}
      />
    );
  }
}

MediaManagementConnector.propTypes = {
  fetchMediaManagementSettings: PropTypes.func.isRequired,
  setMediaManagementSettingsValue: PropTypes.func.isRequired,
  saveMediaManagementSettings: PropTypes.func.isRequired,
  saveNamingSettings: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MediaManagementConnector);
