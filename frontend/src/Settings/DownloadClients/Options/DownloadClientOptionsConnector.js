import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { fetchDownloadClientOptions, setDownloadClientOptionsValue, saveDownloadClientOptions } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import connectSection from 'Store/connectSection';
import DownloadClientOptions from './DownloadClientOptions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSettingsSectionSelector(),
    (advancedSettings, sectionSettings) => {
      return {
        advancedSettings,
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  fetchDownloadClientOptions,
  setDownloadClientOptionsValue,
  saveDownloadClientOptions,
  clearPendingChanges
};

class DownloadClientOptionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchDownloadClientOptions();
  }

  componentDidUpdate(prevProps) {
    if (this.props.hasPendingChanges !== prevProps.hasPendingChanges) {
      this.props.onHasPendingChange(this.props.hasPendingChanges);
    }
  }

  componentWillUnmount() {
    this.props.clearPendingChanges({ section: this.props.section });
  }

  //
  // Control

  save = () => {
    this.props.saveDownloadClientOptions();
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setDownloadClientOptionsValue({ name, value });
  }

  //
  // Render

  render() {
    return (
      <DownloadClientOptions
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

DownloadClientOptionsConnector.propTypes = {
  section: PropTypes.string.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  fetchDownloadClientOptions: PropTypes.func.isRequired,
  setDownloadClientOptionsValue: PropTypes.func.isRequired,
  saveDownloadClientOptions: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired,
  onHasPendingChange: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  { withRef: true },
  { section: 'downloadClientOptions' }
)(DownloadClientOptionsConnector);
