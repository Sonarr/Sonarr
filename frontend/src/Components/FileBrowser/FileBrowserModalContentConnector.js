import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPaths, fetchPaths } from 'Store/Actions/pathActions';
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
  dispatchFetchPaths: fetchPaths,
  dispatchClearPaths: clearPaths
};

class FileBrowserModalContentConnector extends Component {

  // Lifecycle

  componentDidMount() {
    const {
      value,
      includeFiles,
      dispatchFetchPaths
    } = this.props;

    dispatchFetchPaths({
      path: value,
      allowFoldersWithoutTrailingSlashes: true,
      includeFiles
    });
  }

  //
  // Listeners

  onFetchPaths = (path) => {
    const {
      includeFiles,
      dispatchFetchPaths
    } = this.props;

    dispatchFetchPaths({
      path,
      allowFoldersWithoutTrailingSlashes: true,
      includeFiles
    });
  };

  onClearPaths = () => {
    // this.props.dispatchClearPaths();
  };

  onModalClose = () => {
    this.props.dispatchClearPaths();
    this.props.onModalClose();
  };

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
  includeFiles: PropTypes.bool.isRequired,
  dispatchFetchPaths: PropTypes.func.isRequired,
  dispatchClearPaths: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

FileBrowserModalContentConnector.defaultProps = {
  includeFiles: false
};

export default connect(createMapStateToProps, mapDispatchToProps)(FileBrowserModalContentConnector);
