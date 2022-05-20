import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearPaths, fetchPaths } from 'Store/Actions/pathActions';
import PathInput from './PathInput';

function createMapStateToProps() {
  return createSelector(
    (state) => state.paths,
    (paths) => {
      const {
        currentPath,
        directories,
        files
      } = paths;

      const filteredPaths = _.filter([...directories, ...files], ({ path }) => {
        return path.toLowerCase().startsWith(currentPath.toLowerCase());
      });

      return {
        paths: filteredPaths
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchPaths: fetchPaths,
  dispatchClearPaths: clearPaths
};

class PathInputConnector extends Component {

  //
  // Listeners

  onFetchPaths = (path) => {
    const {
      includeFiles,
      dispatchFetchPaths
    } = this.props;

    dispatchFetchPaths({
      path,
      includeFiles
    });
  };

  onClearPaths = () => {
    this.props.dispatchClearPaths();
  };

  //
  // Render

  render() {
    return (
      <PathInput
        onFetchPaths={this.onFetchPaths}
        onClearPaths={this.onClearPaths}
        {...this.props}
      />
    );
  }
}

PathInputConnector.propTypes = {
  includeFiles: PropTypes.bool.isRequired,
  dispatchFetchPaths: PropTypes.func.isRequired,
  dispatchClearPaths: PropTypes.func.isRequired
};

PathInputConnector.defaultProps = {
  includeFiles: false
};

export default connect(createMapStateToProps, mapDispatchToProps)(PathInputConnector);
