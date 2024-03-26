import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import {
  saveDownloadClient,
  setDownloadClientFieldValue,
  setDownloadClientValue,
  testDownloadClient,
  toggleAdvancedSettings
} from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditDownloadClientModalContent from './EditDownloadClientModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('downloadClients'),
    (advancedSettings, downloadClient) => {
      return {
        advancedSettings,
        ...downloadClient
      };
    }
  );
}

const mapDispatchToProps = {
  setDownloadClientValue,
  setDownloadClientFieldValue,
  saveDownloadClient,
  testDownloadClient,
  toggleAdvancedSettings
};

class EditDownloadClientModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setDownloadClientValue({ name, value });
  };

  onFieldChange = ({ name, value }) => {
    this.props.setDownloadClientFieldValue({ name, value });
  };

  onSavePress = () => {
    this.props.saveDownloadClient({ id: this.props.id });
  };

  onTestPress = () => {
    this.props.testDownloadClient({ id: this.props.id });
  };

  onAdvancedSettingsPress = () => {
    this.props.toggleAdvancedSettings();
  };

  //
  // Render

  render() {
    return (
      <EditDownloadClientModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onTestPress={this.onTestPress}
        onAdvancedSettingsPress={this.onAdvancedSettingsPress}
        onInputChange={this.onInputChange}
        onFieldChange={this.onFieldChange}
      />
    );
  }
}

EditDownloadClientModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setDownloadClientValue: PropTypes.func.isRequired,
  setDownloadClientFieldValue: PropTypes.func.isRequired,
  saveDownloadClient: PropTypes.func.isRequired,
  testDownloadClient: PropTypes.func.isRequired,
  toggleAdvancedSettings: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditDownloadClientModalContentConnector);
