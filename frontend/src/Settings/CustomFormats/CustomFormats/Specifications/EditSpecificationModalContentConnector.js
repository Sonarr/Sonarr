import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearCustomFormatSpecificationPending, saveCustomFormatSpecification, setCustomFormatSpecificationFieldValue, setCustomFormatSpecificationValue } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditSpecificationModalContent from './EditSpecificationModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('customFormatSpecifications'),
    (advancedSettings, specification) => {
      return {
        advancedSettings,
        ...specification
      };
    }
  );
}

const mapDispatchToProps = {
  setCustomFormatSpecificationValue,
  setCustomFormatSpecificationFieldValue,
  saveCustomFormatSpecification,
  clearCustomFormatSpecificationPending
};

class EditSpecificationModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setCustomFormatSpecificationValue({ name, value });
  };

  onFieldChange = ({ name, value }) => {
    this.props.setCustomFormatSpecificationFieldValue({ name, value });
  };

  onCancelPress = () => {
    this.props.clearCustomFormatSpecificationPending();
    this.props.onModalClose();
  };

  onSavePress = () => {
    this.props.saveCustomFormatSpecification({ id: this.props.id });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <EditSpecificationModalContent
        {...this.props}
        onCancelPress={this.onCancelPress}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
        onFieldChange={this.onFieldChange}
      />
    );
  }
}

EditSpecificationModalContentConnector.propTypes = {
  id: PropTypes.number,
  item: PropTypes.object.isRequired,
  setCustomFormatSpecificationValue: PropTypes.func.isRequired,
  setCustomFormatSpecificationFieldValue: PropTypes.func.isRequired,
  clearCustomFormatSpecificationPending: PropTypes.func.isRequired,
  saveCustomFormatSpecification: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditSpecificationModalContentConnector);
