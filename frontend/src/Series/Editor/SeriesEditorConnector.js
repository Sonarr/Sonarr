import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { saveSeriesEditor, setSeriesEditorFilter, setSeriesEditorSort, setSeriesEditorTableOption } from 'Store/Actions/seriesEditorActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import SeriesEditor from './SeriesEditor';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('series', 'seriesEditor'),
    createCommandExecutingSelector(commandNames.RENAME_SERIES),
    (series, isOrganizingSeries) => {
      return {
        isOrganizingSeries,
        ...series
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetSeriesEditorSort: setSeriesEditorSort,
  dispatchSetSeriesEditorFilter: setSeriesEditorFilter,
  dispatchSetSeriesEditorTableOption: setSeriesEditorTableOption,
  dispatchSaveSeriesEditor: saveSeriesEditor,
  dispatchFetchRootFolders: fetchRootFolders,
  dispatchExecuteCommand: executeCommand
};

class SeriesEditorConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchRootFolders();
  }

  //
  // Listeners

  onSortPress = (sortKey) => {
    this.props.dispatchSetSeriesEditorSort({ sortKey });
  };

  onFilterSelect = (selectedFilterKey) => {
    this.props.dispatchSetSeriesEditorFilter({ selectedFilterKey });
  };

  onTableOptionChange = (payload) => {
    this.props.dispatchSetSeriesEditorTableOption(payload);
  };

  onSaveSelected = (payload) => {
    this.props.dispatchSaveSeriesEditor(payload);
  };

  onMoveSelected = (payload) => {
    this.props.dispatchExecuteCommand({
      name: commandNames.MOVE_SERIES,
      ...payload
    });
  };

  //
  // Render

  render() {
    return (
      <SeriesEditor
        {...this.props}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onSaveSelected={this.onSaveSelected}
        onTableOptionChange={this.onTableOptionChange}
      />
    );
  }
}

SeriesEditorConnector.propTypes = {
  dispatchSetSeriesEditorSort: PropTypes.func.isRequired,
  dispatchSetSeriesEditorFilter: PropTypes.func.isRequired,
  dispatchSetSeriesEditorTableOption: PropTypes.func.isRequired,
  dispatchSaveSeriesEditor: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesEditorConnector);
