import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchCustomFormatSpecifications } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import ExportCustomFormatModalContent from './ExportCustomFormatModalContent';

const omittedProperties = ['id', 'implementationName', 'infoLink'];

function replacer(key, value) {
  if (omittedProperties.includes(key)) {
    return undefined;
  }

  // provider fields
  if (key === 'fields') {
    return value.reduce((acc, cur) => {
      acc[cur.name] = cur.value;
      return acc;
    }, {});
  }

  // regular setting values
  if (value.hasOwnProperty('value')) {
    return value.value;
  }

  return value;
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('customFormats'),
    (state) => state.settings.customFormatSpecifications,
    (advancedSettings, customFormat, specifications) => {
      const json = customFormat.item ? JSON.stringify(customFormat.item, replacer, 2) : '';
      return {
        advancedSettings,
        ...customFormat,
        json,
        specificationsPopulated: specifications.isPopulated,
        specifications: specifications.items
      };
    }
  );
}

const mapDispatchToProps = {
  fetchCustomFormatSpecifications
};

class ExportCustomFormatModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      id
    } = this.props;
    this.props.fetchCustomFormatSpecifications({ id });
  }

  //
  // Render

  render() {
    return (
      <ExportCustomFormatModalContent
        {...this.props}
      />
    );
  }
}

ExportCustomFormatModalContentConnector.propTypes = {
  id: PropTypes.number,
  fetchCustomFormatSpecifications: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ExportCustomFormatModalContentConnector);
