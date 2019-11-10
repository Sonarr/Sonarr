import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchImportLists, deleteImportList } from 'Store/Actions/settingsActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import ImportLists from './ImportLists';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.importLists,
    (importLists) => {
      return {
        ...importLists
      };
    }
  );
}

const mapDispatchToProps = {
  fetchImportLists,
  deleteImportList,
  fetchRootFolders
};

class ListsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchImportLists();
    this.props.fetchRootFolders();
  }

  //
  // Listeners

  onConfirmDeleteImportList = (id) => {
    this.props.deleteImportList({ id });
  }

  //
  // Render

  render() {
    return (
      <ImportLists
        {...this.props}
        onConfirmDeleteImportList={this.onConfirmDeleteImportList}
      />
    );
  }
}

ListsConnector.propTypes = {
  fetchImportLists: PropTypes.func.isRequired,
  deleteImportList: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ListsConnector);
