import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { fetchGeneralSettings, saveGeneralSettings, setGeneralSettingsValue } from 'Store/Actions/settingsActions';
import { fetchStatus } from 'Store/Actions/systemActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import AuthenticationRequiredModalContent from './AuthenticationRequiredModalContent';

const SECTION = 'general';

function createMapStateToProps() {
  return createSelector(
    createSettingsSectionSelector(SECTION),
    (sectionSettings) => {
      return {
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchClearPendingChanges: clearPendingChanges,
  dispatchSetGeneralSettingsValue: setGeneralSettingsValue,
  dispatchSaveGeneralSettings: saveGeneralSettings,
  dispatchFetchGeneralSettings: fetchGeneralSettings,
  dispatchFetchStatus: fetchStatus
};

class AuthenticationRequiredModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchGeneralSettings();
  }

  componentWillUnmount() {
    this.props.dispatchClearPendingChanges({ section: `settings.${SECTION}` });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetGeneralSettingsValue({ name, value });
  };

  onSavePress = () => {
    this.props.dispatchSaveGeneralSettings();
  };

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      dispatchFetchGeneralSettings,
      dispatchSetGeneralSettingsValue,
      dispatchSaveGeneralSettings,
      ...otherProps
    } = this.props;

    return (
      <AuthenticationRequiredModalContent
        {...otherProps}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
      />
    );
  }
}

AuthenticationRequiredModalContentConnector.propTypes = {
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  dispatchFetchGeneralSettings: PropTypes.func.isRequired,
  dispatchSetGeneralSettingsValue: PropTypes.func.isRequired,
  dispatchSaveGeneralSettings: PropTypes.func.isRequired,
  dispatchFetchStatus: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AuthenticationRequiredModalContentConnector);
