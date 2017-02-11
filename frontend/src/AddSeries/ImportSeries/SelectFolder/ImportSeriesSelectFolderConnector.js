import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { push } from 'react-router-redux';
import { fetchRootFolders, addRootFolder, deleteRootFolder } from 'Store/Actions/rootFolderActions';
import ImportSeriesSelectFolder from './ImportSeriesSelectFolder';

function createMapStateToProps() {
  return createSelector(
    (state) => state.rootFolders,
    (rootFolders) => {
      return rootFolders;
    }
  );
}

const mapDispatchToProps = {
  fetchRootFolders,
  addRootFolder,
  deleteRootFolder,
  push
};

class ImportSeriesSelectFolderConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchRootFolders();
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      isSaving,
      saveError
    } = this.props;

    if (prevProps.isSaving && !isSaving && !saveError) {
      const newRootFolders = _.differenceBy(items, prevProps.items, (item) => item.id);

      if (newRootFolders.length === 1) {
        this.props.push(`${window.Sonarr.urlBase}/add/import/${newRootFolders[0].id}`);
      }
    }
  }

  //
  // Listeners

  onNewRootFolderSelect = (path) => {
    this.props.addRootFolder({ path });
  }

  onDeleteRootFolderPress = (id) => {
    this.props.deleteRootFolder({ id });
  }

  //
  // Render

  render() {
    return (
      <ImportSeriesSelectFolder
        {...this.props}
        onNewRootFolderSelect={this.onNewRootFolderSelect}
        onDeleteRootFolderPress={this.onDeleteRootFolderPress}
      />
    );
  }
}

ImportSeriesSelectFolderConnector.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchRootFolders: PropTypes.func.isRequired,
  addRootFolder: PropTypes.func.isRequired,
  deleteRootFolder: PropTypes.func.isRequired,
  push: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportSeriesSelectFolderConnector);
