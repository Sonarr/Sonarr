import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchPaths, clearPaths } from 'Store/Actions/pathActions';
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
  fetchPaths,
  clearPaths
};

class PathInputConnector extends Component {

  //
  // Listeners

  onFetchPaths = (path) => {
    this.props.fetchPaths({ path });
  }

  onClearPaths = () => {
    this.props.clearPaths();
  }

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
  fetchPaths: PropTypes.func.isRequired,
  clearPaths: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(PathInputConnector);
