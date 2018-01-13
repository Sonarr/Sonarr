import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { fetchNamingSettings, setNamingSettingsValue, fetchNamingExamples } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import Naming from './Naming';

const SECTION = 'naming';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    (state) => state.settings.namingExamples,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, examples, sectionSettings) => {
      return {
        advancedSettings,
        examples: examples.item,
        examplesPopulated: !_.isEmpty(examples.item),
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  fetchNamingSettings,
  setNamingSettingsValue,
  fetchNamingExamples,
  clearPendingChanges
};

class NamingConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._namingExampleTimeout = null;
  }

  componentDidMount() {
    this.props.fetchNamingSettings();
    this.props.fetchNamingExamples();
  }

  componentWillUnmount() {
    this.props.clearPendingChanges({ section: SECTION });
  }

  //
  // Control

  _fetchNamingExamples = () => {
    this.props.fetchNamingExamples();
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setNamingSettingsValue({ name, value });

    if (this._namingExampleTimeout) {
      clearTimeout(this._namingExampleTimeout);
    }

    this._namingExampleTimeout = setTimeout(this._fetchNamingExamples, 1000);
  }

  //
  // Render

  render() {
    return (
      <Naming
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        {...this.props}
      />
    );
  }
}

NamingConnector.propTypes = {
  fetchNamingSettings: PropTypes.func.isRequired,
  setNamingSettingsValue: PropTypes.func.isRequired,
  fetchNamingExamples: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(NamingConnector);
