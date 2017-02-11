import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchPaths, clearPaths } from 'Store/Actions/pathActions';
import FileBrowserModalContent from './FileBrowserModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.paths,
    (paths) => {
      const {
        parent,
        currentPath,
        directories,
        files
      } = paths;

      const filteredPaths = _.filter([...directories, ...files], ({ path }) => {
        return path.toLowerCase().startsWith(currentPath.toLowerCase());
      });

      return {
        parent,
        currentPath,
        directories,
        files,
        paths: filteredPaths
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
    this.props.fetchPaths({ path: this.props.value });
  }

  //
  // Listeners

  onFetchPaths = (path) => {
    this.props.fetchPaths({ path });
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
