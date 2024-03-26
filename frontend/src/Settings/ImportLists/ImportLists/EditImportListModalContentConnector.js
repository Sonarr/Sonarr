import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import {
  saveImportList,
  setImportListFieldValue,
  setImportListValue,
  testImportList,
  toggleAdvancedSettings
} from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditImportListModalContent from './EditImportListModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('importLists'),
    (advancedSettings, importList) => {
      return {
        advancedSettings,
        ...importList
      };
    }
  );
}

const mapDispatchToProps = {
  setImportListValue,
  setImportListFieldValue,
  saveImportList,
  testImportList,
  toggleAdvancedSettings
};

class EditImportListModalContentConnector extends Component {

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
    this.props.setImportListValue({ name, value });
  };

  onFieldChange = ({ name, value }) => {
    this.props.setImportListFieldValue({ name, value });
  };

  onSavePress = () => {
    this.props.saveImportList({ id: this.props.id });
  };

  onTestPress = () => {
    this.props.testImportList({ id: this.props.id });
  };

  onAdvancedSettingsPress = () => {
    this.props.toggleAdvancedSettings();
  };

  //
  // Render

  render() {
    return (
      <EditImportListModalContent
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

EditImportListModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setImportListValue: PropTypes.func.isRequired,
  setImportListFieldValue: PropTypes.func.isRequired,
  saveImportList: PropTypes.func.isRequired,
  testImportList: PropTypes.func.isRequired,
  toggleAdvancedSettings: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditImportListModalContentConnector);
