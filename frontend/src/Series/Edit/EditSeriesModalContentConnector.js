import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveSeries, setSeriesValue } from 'Store/Actions/seriesActions';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import combinePath from 'Utilities/String/combinePath';
import EditSeriesModalContent from './EditSeriesModalContent';

function createIsPathChangingSelector() {
  return createSelector(
    (state) => state.series.pendingChanges,
    createSeriesSelector(),
    (pendingChanges, series) => {
      const path = pendingChanges.path;

      if (path == null) {
        return false;
      }

      return series.path !== path;
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.series,
    (state) => state.system.status.item.isWindows,
    createSeriesSelector(),
    createIsPathChangingSelector(),
    (seriesState, isWindows, series, isPathChanging) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = seriesState;

      const seriesSettings = _.pick(series, [
        'monitored',
        'seasonFolder',
        'qualityProfileId',
        'seriesType',
        'path',
        'rootFolderPath',
        'tags'
      ]);

      const settings = selectSettings(seriesSettings, pendingChanges, saveError);

      return {
        title: series.title,
        isWindows,
        isSaving,
        saveError,
        isPathChanging,
        originalPath: series.path,
        item: settings.settings,
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
    const {
      dispatchSetSeriesValue,
      isWindows,
      title,
      item: { path }
    } = this.props;

    dispatchSetSeriesValue({ name, value });

    // Also update the path if the root folder path changes
    if (name === 'rootFolderPath' && value !== 'noChange' && path.value.indexOf(value) === -1) {
      const newPath = combinePath(isWindows, value, [title]);
      dispatchSetSeriesValue({ name: 'path', value: newPath });
    }
  };

  onSavePress = (moveFiles) => {
    this.props.dispatchSaveSeries({
      id: this.props.seriesId,
      moveFiles
    });
  };

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
  title: PropTypes.string.isRequired,
  item: PropTypes.object.isRequired,
  seriesId: PropTypes.number,
  isWindows: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetSeriesValue: PropTypes.func.isRequired,
  dispatchSaveSeries: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditSeriesModalContentConnector);
