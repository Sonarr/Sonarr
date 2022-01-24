import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cloneCustomFormatSpecification, deleteCustomFormatSpecification, fetchCustomFormatSpecifications, saveCustomFormat, setCustomFormatValue } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditCustomFormatModalContent from './EditCustomFormatModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('customFormats'),
    (state) => state.settings.customFormatSpecifications,
    (advancedSettings, customFormat, specifications) => {
      return {
        advancedSettings,
        ...customFormat,
        specificationsPopulated: specifications.isPopulated,
        specifications: specifications.items
      };
    }
  );
}

const mapDispatchToProps = {
  setCustomFormatValue,
  saveCustomFormat,
  fetchCustomFormatSpecifications,
  cloneCustomFormatSpecification,
  deleteCustomFormatSpecification
};

class EditCustomFormatModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      id,
      tagsFromId
    } = this.props;
    this.props.fetchCustomFormatSpecifications({ id: tagsFromId || id });
  }

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setCustomFormatValue({ name, value });
  };

  onSavePress = () => {
    this.props.saveCustomFormat({ id: this.props.id });
  };

  onCloneSpecificationPress = (id) => {
    this.props.cloneCustomFormatSpecification({ id });
  };

  onConfirmDeleteSpecification = (id) => {
    this.props.deleteCustomFormatSpecification({ id });
  };

  //
  // Render

  render() {
    return (
      <EditCustomFormatModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
        onCloneSpecificationPress={this.onCloneSpecificationPress}
        onConfirmDeleteSpecification={this.onConfirmDeleteSpecification}
      />
    );
  }
}

EditCustomFormatModalContentConnector.propTypes = {
  id: PropTypes.number,
  tagsFromId: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setCustomFormatValue: PropTypes.func.isRequired,
  saveCustomFormat: PropTypes.func.isRequired,
  fetchCustomFormatSpecifications: PropTypes.func.isRequired,
  cloneCustomFormatSpecification: PropTypes.func.isRequired,
  deleteCustomFormatSpecification: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditCustomFormatModalContentConnector);
