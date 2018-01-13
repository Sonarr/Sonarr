import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { fetchIndexerOptions, setIndexerOptionsValue, saveIndexerOptions } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import connectSection from 'Store/connectSection';
import IndexerOptions from './IndexerOptions';

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
  dispatchFetchIndexerOptions: fetchIndexerOptions,
  dispatchSetIndexerOptionsValue: setIndexerOptionsValue,
  dispatchSaveIndexerOptions: saveIndexerOptions,
  dispatchClearPendingChanges: clearPendingChanges
};

class IndexerOptionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchIndexerOptions,
      dispatchSaveIndexerOptions,
      onChildMounted
    } = this.props;

    dispatchFetchIndexerOptions();
    onChildMounted(dispatchSaveIndexerOptions);
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
    this.props.dispatchSetIndexerOptionsValue({ name, value });
  }

  //
  // Render

  render() {
    return (
      <IndexerOptions
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

IndexerOptionsConnector.propTypes = {
  section: PropTypes.string.isRequired,
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchIndexerOptions: PropTypes.func.isRequired,
  dispatchSetIndexerOptionsValue: PropTypes.func.isRequired,
  dispatchSaveIndexerOptions: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'settings.indexerOptions' }
)(IndexerOptionsConnector);
