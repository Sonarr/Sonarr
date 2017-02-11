import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import { setNotificationValue, setNotificationFieldValue, saveNotification, testNotification } from 'Store/Actions/settingsActions';
import connectSection from 'Store/connectSection';
import EditNotificationModalContent from './EditNotificationModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector(),
    (advancedSettings, notification) => {
      return {
        advancedSettings,
        ...notification
      };
    }
  );
}

const mapDispatchToProps = {
  setNotificationValue,
  setNotificationFieldValue,
  saveNotification,
  testNotification
};

class EditNotificationModalContentConnector extends Component {

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
    this.props.setNotificationValue({ name, value });
  }

  onFieldChange = ({ name, value }) => {
    this.props.setNotificationFieldValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveNotification({ id: this.props.id });
  }

  onTestPress = () => {
    this.props.testNotification({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditNotificationModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onTestPress={this.onTestPress}
        onInputChange={this.onInputChange}
        onFieldChange={this.onFieldChange}
      />
    );
  }
}

EditNotificationModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setNotificationValue: PropTypes.func.isRequired,
  setNotificationFieldValue: PropTypes.func.isRequired,
  saveNotification: PropTypes.func.isRequired,
  testNotification: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'notifications' }
)(EditNotificationModalContentConnector);
