import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { setUISettingsValue, saveUISettings, fetchUISettings } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import UISettings from './UISettings';

const SECTION = 'ui';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, sectionSettings) => {
      return {
        advancedSettings,
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  setUISettingsValue,
  saveUISettings,
  fetchUISettings,
  clearPendingChanges
};

class UISettingsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchUISettings();
  }

  componentWillUnmount() {
    this.props.clearPendingChanges({ section: SECTION });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setUISettingsValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveUISettings();
  }

  //
  // Render

  render() {
    return (
      <UISettings
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        {...this.props}
      />
    );
  }
}

UISettingsConnector.propTypes = {
  setUISettingsValue: PropTypes.func.isRequired,
  saveUISettings: PropTypes.func.isRequired,
  fetchUISettings: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(UISettingsConnector);
