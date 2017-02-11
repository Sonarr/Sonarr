import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandSelector from 'Store/Selectors/createCommandSelector';
import { setSeriesEditorSort, setSeriesEditorFilter, saveSeriesEditor } from 'Store/Actions/seriesEditorActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import * as commandNames from 'Commands/commandNames';
import SeriesEditor from './SeriesEditor';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.languageProfiles,
    createClientSideCollectionSelector(),
    createCommandSelector(commandNames.RENAME_SERIES),
    (languageProfiles, series, isOrganizingSeries) => {
      return {
        isOrganizingSeries,
        showLanguageProfile: languageProfiles.items.length > 1,
        ...series
      };
    }
  );
}

const mapDispatchToProps = {
  setSeriesEditorSort,
  setSeriesEditorFilter,
  saveSeriesEditor,
  fetchRootFolders
};

class SeriesEditorConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchRootFolders();
  }

  //
  // Listeners

  onSortPress = (sortKey) => {
    this.props.setSeriesEditorSort({ sortKey });
  }

  onFilterSelect = (filterKey, filterValue, filterType) => {
    this.props.setSeriesEditorFilter({ filterKey, filterValue, filterType });
  }

  onSaveSelected = (payload) => {
    this.props.saveSeriesEditor(payload);
  }

  //
  // Render

  render() {
    return (
      <SeriesEditor
        {...this.props}
        onSortPress={this.onSortPress}
        onFilterSelect={this.onFilterSelect}
        onSaveSelected={this.onSaveSelected}
      />
    );
  }
}

SeriesEditorConnector.propTypes = {
  setSeriesEditorSort: PropTypes.func.isRequired,
  setSeriesEditorFilter: PropTypes.func.isRequired,
  saveSeriesEditor: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'series', uiSection: 'seriesEditor' }
)(SeriesEditorConnector);
