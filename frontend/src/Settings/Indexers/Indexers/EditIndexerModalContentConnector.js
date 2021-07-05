import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveIndexer, setIndexerFieldValue, setIndexerValue, testIndexer, toggleAdvancedSettings } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditIndexerModalContent from './EditIndexerModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('indexers'),
    (advancedSettings, indexer) => {
      return {
        advancedSettings,
        ...indexer
      };
    }
  );
}

const mapDispatchToProps = {
  setIndexerValue,
  setIndexerFieldValue,
  saveIndexer,
  testIndexer,
  toggleAdvancedSettings
};

class EditIndexerModalContentConnector extends Component {

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
    this.props.setIndexerValue({ name, value });
  };

  onFieldChange = ({ name, value }) => {
    this.props.setIndexerFieldValue({ name, value });
  };

  onSavePress = () => {
    this.props.saveIndexer({ id: this.props.id });
  };

  onTestPress = () => {
    this.props.testIndexer({ id: this.props.id });
  };

  onAdvancedSettingsPress = () => {
    this.props.toggleAdvancedSettings();
  };

  //
  // Render

  render() {
    return (
      <EditIndexerModalContent
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

EditIndexerModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setIndexerValue: PropTypes.func.isRequired,
  setIndexerFieldValue: PropTypes.func.isRequired,
  toggleAdvancedSettings: PropTypes.func.isRequired,
  saveIndexer: PropTypes.func.isRequired,
  testIndexer: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditIndexerModalContentConnector);
