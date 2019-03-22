import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchPaths, clearPaths } from 'Store/Actions/pathActions';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import FileBrowserModalContent from './FileBrowserModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.paths,
    createSystemStatusSelector(),
    (paths, systemStatus) => {
      const {
        isFetching,
        isPopulated,
        error,
        parent,
        currentPath,
        directories,
        files
      } = paths;

      const filteredPaths = _.filter([...directories, ...files], ({ path }) => {
        return path.toLowerCase().startsWith(currentPath.toLowerCase());
      });

      return {
        isFetching,
        isPopulated,
        error,
        parent,
        currentPath,
        directories,
        files,
        paths: filteredPaths,
        isWindowsService: systemStatus.isWindows && systemStatus.mode === 'service'
      };
    }
  );
}

const mapDispatchToProps = {
  fetchPaths,
  clearPaths
};

class FileBrowserModalContentConnector extends Component {

  // Lifecycle

  componentDidMount() {
    this.props.fetchPaths({
      path: this.props.value,
      allowFoldersWithoutTrailingSlashes: true
    });
  }

  //
  // Listeners

  onFetchPaths = (path) => {
    this.props.fetchPaths({
      path,
      allowFoldersWithoutTrailingSlashes: true
    });
  }

  onClearPaths = () => {
    // this.props.clearPaths();
  }

  onModalClose = () => {
    this.props.clearPaths();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <FileBrowserModalContent
        onFetchPaths={this.onFetchPaths}
        onClearPaths={this.onClearPaths}
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

FileBrowserModalContentConnector.propTypes = {
  value: PropTypes.string,
  fetchPaths: PropTypes.func.isRequired,
  clearPaths: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(FileBrowserModalContentConnector);
