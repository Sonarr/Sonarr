import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { fetchIndexerOptions, setIndexerOptionsValue, saveIndexerOptions } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import IndexerOptions from './IndexerOptions';

const SECTION = 'indexerOptions';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSettingsSectionSelector(SECTION),
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
    this.props.dispatchClearPendingChanges({ section: SECTION });
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
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchIndexerOptions: PropTypes.func.isRequired,
  dispatchSetIndexerOptionsValue: PropTypes.func.isRequired,
  dispatchSaveIndexerOptions: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(IndexerOptionsConnector);
