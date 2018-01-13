import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import { setSeriesValue, saveSeries } from 'Store/Actions/seriesActions';
import EditSeriesModalContent from './EditSeriesModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series,
    (state) => state.settings.languageProfiles,
    createSeriesSelector(),
    (seriesState, languageProfiles, series) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = seriesState;

      const seriesSettings = _.pick(series, [
        'monitored',
        'seasonFolder',
        'qualityProfileId',
        'languageProfileId',
        'seriesType',
        'path',
        'tags'
      ]);

      const settings = selectSettings(seriesSettings, pendingChanges, saveError);

      return {
        title: series.title,
        isSaving,
        saveError,
        isPathChanging: pendingChanges.hasOwnProperty('path'),
        originalPath: series.path,
        item: settings.settings,
        showLanguageProfile: languageProfiles.items.length > 1,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetSeriesValue: setSeriesValue,
  dispatchSaveSeries: saveSeries
};

class EditSeriesModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetSeriesValue({ name, value });
  }

  onSavePress = (moveFiles) => {
    this.props.dispatchSaveSeries({
      id: this.props.seriesId,
      moveFiles
    });
  }

  //
  // Render

  render() {
    return (
      <EditSeriesModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        onMoveSeriesPress={this.onMoveSeriesPress}
      />
    );
  }
}

EditSeriesModalContentConnector.propTypes = {
  seriesId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetSeriesValue: PropTypes.func.isRequired,
  dispatchSaveSeries: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditSeriesModalContentConnector);
