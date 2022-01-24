import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { clearCustomFormatSpecificationPending, deleteAllCustomFormatSpecification, fetchCustomFormatSpecificationSchema, saveCustomFormatSpecification, selectCustomFormatSpecificationSchema, setCustomFormatSpecificationFieldValue, setCustomFormatSpecificationValue, setCustomFormatValue } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import ImportCustomFormatModalContent from './ImportCustomFormatModalContent';

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
        specificationSchema: specifications.schema
      };
    }
  );
}

const mapDispatchToProps = {
  deleteAllCustomFormatSpecification,
  clearCustomFormatSpecificationPending,
  clearPendingChanges,
  saveCustomFormatSpecification,
  selectCustomFormatSpecificationSchema,
  setCustomFormatSpecificationFieldValue,
  setCustomFormatSpecificationValue,
  setCustomFormatValue,
  fetchCustomFormatSpecificationSchema
};

class ImportCustomFormatModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchCustomFormatSpecificationSchema();
  }

  //
  // Listeners

  clearPending = () => {
    this.props.clearPendingChanges({ section: 'settings.customFormats' });
    this.props.clearCustomFormatSpecificationPending();
    this.props.deleteAllCustomFormatSpecification();
  };

  onImportPress = (payload) => {

    this.clearPending();

    try {
      const cf = JSON.parse(payload);
      this.parseCf(cf);
    } catch (err) {
      this.clearPending();
      return {
        message: err.message,
        detailedMessage: err.stack
      };
    }

    return null;
  };

  parseCf = (cf) => {
    for (const [key, value] of Object.entries(cf)) {
      if (key === 'specifications') {
        for (const spec of value) {
          this.parseSpecification(spec);
        }
      } else if (key !== 'id') {
        this.props.setCustomFormatValue({ name: key, value });
      }
    }
  };

  parseSpecification = (spec) => {
    const selectedImplementation = _.find(this.props.specificationSchema, { implementation: spec.implementation });

    if (!selectedImplementation) {
      throw new Error(`Unknown Custom Format condition '${spec.implementation}'`);
    }

    this.props.selectCustomFormatSpecificationSchema({ implementation: spec.implementation });

    for (const [key, value] of Object.entries(spec)) {
      if (key === 'fields') {
        this.parseFields(value, selectedImplementation);
      } else if (key !== 'id') {
        this.props.setCustomFormatSpecificationValue({ name: key, value });
      }
    }

    this.props.saveCustomFormatSpecification();
  };

  parseFields = (fields, schema) => {
    for (const [key, value] of Object.entries(fields)) {
      const field = _.find(schema.fields, { name: key });
      if (!field) {
        throw new Error(`Unknown option '${key}' for condition '${schema.implementationName}'`);
      }

      this.props.setCustomFormatSpecificationFieldValue({ name: key, value });
    }
  };

  //
  // Render

  render() {
    return (
      <ImportCustomFormatModalContent
        {...this.props}
        onImportPress={this.onImportPress}
      />
    );
  }
}

ImportCustomFormatModalContentConnector.propTypes = {
  specificationSchema: PropTypes.arrayOf(PropTypes.object).isRequired,
  clearPendingChanges: PropTypes.func.isRequired,
  deleteAllCustomFormatSpecification: PropTypes.func.isRequired,
  clearCustomFormatSpecificationPending: PropTypes.func.isRequired,
  saveCustomFormatSpecification: PropTypes.func.isRequired,
  fetchCustomFormatSpecificationSchema: PropTypes.func.isRequired,
  selectCustomFormatSpecificationSchema: PropTypes.func.isRequired,
  setCustomFormatSpecificationValue: PropTypes.func.isRequired,
  setCustomFormatSpecificationFieldValue: PropTypes.func.isRequired,
  setCustomFormatValue: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportCustomFormatModalContentConnector);
