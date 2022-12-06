import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearAutoTaggingSpecificationPending, saveAutoTaggingSpecification, setAutoTaggingSpecificationFieldValue, setAutoTaggingSpecificationValue } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditSpecificationModalContent from './EditSpecificationModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('autoTaggingSpecifications'),
    (advancedSettings, specification) => {
      return {
        advancedSettings,
        ...specification
      };
    }
  );
}

const mapDispatchToProps = {
  setAutoTaggingSpecificationValue,
  setAutoTaggingSpecificationFieldValue,
  saveAutoTaggingSpecification,
  clearAutoTaggingSpecificationPending
};

class EditSpecificationModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setAutoTaggingSpecificationValue({ name, value });
  };

  onFieldChange = ({ name, value }) => {
    this.props.setAutoTaggingSpecificationFieldValue({ name, value });
  };

  onCancelPress = () => {
    this.props.clearAutoTaggingSpecificationPending();
    this.props.onModalClose();
  };

  onSavePress = () => {
    this.props.saveAutoTaggingSpecification({ id: this.props.id });
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
  setAutoTaggingSpecificationValue: PropTypes.func.isRequired,
  setAutoTaggingSpecificationFieldValue: PropTypes.func.isRequired,
  clearAutoTaggingSpecificationPending: PropTypes.func.isRequired,
  saveAutoTaggingSpecification: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditSpecificationModalContentConnector);
