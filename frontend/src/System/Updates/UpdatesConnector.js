import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchGeneralSettings } from 'Store/Actions/settingsActions';
import { fetchUpdates } from 'Store/Actions/systemActions';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import * as commandNames from 'Commands/commandNames';
import Updates from './Updates';

function createMapStateToProps() {
  return createSelector(
    (state) => state.app.version,
    createSystemStatusSelector(),
    (state) => state.system.updates,
    (state) => state.settings.general,
    createUISettingsSelector(),
    createCommandExecutingSelector(commandNames.APPLICATION_UPDATE),
    (
      currentVersion,
      status,
      updates,
      generalSettings,
      uiSettings,
      isInstallingUpdate
    ) => {
      const {
        error: updatesError,
        items
      } = updates;

      const isFetching = updates.isFetching || generalSettings.isFetching;
      const isPopulated = updates.isPopulated && generalSettings.isPopulated;

      return {
        currentVersion,
        isFetching,
        isPopulated,
        updatesError,
        generalSettingsError: generalSettings.error,
        items,
        isInstallingUpdate,
        updateMechanism: generalSettings.item.updateMechanism,
        updateMechanismMessage: status.packageUpdateMechanismMessage,
        shortDateFormat: uiSettings.shortDateFormat,
        longDateFormat: uiSettings.longDateFormat,
        timeFormat: uiSettings.timeFormat
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchUpdates: fetchUpdates,
  dispatchFetchGeneralSettings: fetchGeneralSettings,
  dispatchExecuteCommand: executeCommand
};

class UpdatesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchUpdates();
    this.props.dispatchFetchGeneralSettings();
  }

  //
  // Listeners

  onInstallLatestPress = () => {
    this.props.dispatchExecuteCommand({ name: commandNames.APPLICATION_UPDATE });
  }

  //
  // Render

  render() {
    return (
      <Updates
        onInstallLatestPress={this.onInstallLatestPress}
        {...this.props}
      />
    );
  }
}

UpdatesConnector.propTypes = {
  dispatchFetchUpdates: PropTypes.func.isRequired,
  dispatchFetchGeneralSettings: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(UpdatesConnector);
