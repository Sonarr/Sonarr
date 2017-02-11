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
  fetchIndexerOptions,
  setIndexerOptionsValue,
  saveIndexerOptions,
  clearPendingChanges
};

class IndexerOptionsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchIndexerOptions();
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
    this.props.saveIndexerOptions();
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setIndexerOptionsValue({ name, value });
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
  hasPendingChanges: PropTypes.bool.isRequired,
  fetchIndexerOptions: PropTypes.func.isRequired,
  setIndexerOptionsValue: PropTypes.func.isRequired,
  saveIndexerOptions: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired,
  onHasPendingChange: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  { withRef: true },
  { section: 'indexerOptions' }
)(IndexerOptionsConnector);
