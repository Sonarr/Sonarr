import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchUpdates } from 'Store/Actions/systemActions';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import * as commandNames from 'Commands/commandNames';
import Updates from './Updates';

function createMapStateToProps() {
  return createSelector(
    (state) => state.system.updates,
    createUISettingsSelector(),
    createCommandExecutingSelector(commandNames.APPLICATION_UPDATE),
    (updates, uiSettings, isInstallingUpdate) => {
      const {
        isFetching,
        isPopulated,
        error,
        items
      } = updates;

      return {
        isFetching,
        isPopulated,
        error,
        items,
        isInstallingUpdate,
        shortDateFormat: uiSettings.shortDateFormat
      };
    }
  );
}

const mapDispatchToProps = {
  fetchUpdates,
  executeCommand
};

class UpdatesConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchUpdates();
  }

  //
  // Listeners

  onInstallLatestPress = () => {
    this.props.executeCommand({ name: commandNames.APPLICATION_UPDATE });
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
  fetchUpdates: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(UpdatesConnector);
