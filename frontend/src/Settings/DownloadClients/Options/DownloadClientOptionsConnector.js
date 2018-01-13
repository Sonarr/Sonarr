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
  dispatchFetchDownloadClientOptions: fetchDownloadClientOptions,
  dispatchSetDownloadClientOptionsValue: setDownloadClientOptionsValue,
  dispatchSaveDownloadClientOptions: saveDownloadClientOptions,
  dispatchClearPendingChanges: clearPendingChanges
};

class DownloadClientOptionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchDownloadClientOptions,
      dispatchSaveDownloadClientOptions,
      onChildMounted
    } = this.props;

    dispatchFetchDownloadClientOptions();
    onChildMounted(dispatchSaveDownloadClientOptions);
  }

  componentDidUpdate(prevProps) {
    const {
      hasPendingChanges,
      isSaving,
      onChildStateChange
    } = this.props;

    if (
      prevProps.isSaving !== isSaving ||
      prevProps.hasPendingChanges !== hasPendingChanges
    ) {
      onChildStateChange({
        isSaving,
        hasPendingChanges
      });
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearPendingChanges({ section: this.props.section });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetDownloadClientOptionsValue({ name, value });
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
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchDownloadClientOptions: PropTypes.func.isRequired,
  dispatchSetDownloadClientOptionsValue: PropTypes.func.isRequired,
  dispatchSaveDownloadClientOptions: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'settings.downloadClientOptions' }
)(DownloadClientOptionsConnector);
