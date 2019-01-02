import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import RootFolders from './RootFolders';

function createMapStateToProps() {
  return createSelector(
    (state) => state.rootFolders,
    (rootFolders) => {
      return rootFolders;
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchRootFolders: fetchRootFolders
};

class RootFoldersConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchRootFolders();
  }

  //
  // Render

  render() {
    return (
      <RootFolders
        {...this.props}
      />
    );
  }
}

RootFoldersConnector.propTypes = {
  dispatchFetchRootFolders: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(RootFoldersConnector);
