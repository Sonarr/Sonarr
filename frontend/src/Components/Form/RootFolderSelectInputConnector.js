import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addRootFolder } from 'Store/Actions/rootFolderActions';
import RootFolderSelectInput from './RootFolderSelectInput';

const ADD_NEW_KEY = 'addNew';

function createMapStateToProps() {
  return createSelector(
    (state) => state.rootFolders,
    (state, { includeNoChange }) => includeNoChange,
    (rootFolders, includeNoChange) => {
      const values = _.map(rootFolders.items, (rootFolder) => {
        return {
          key: rootFolder.path,
          value: rootFolder.path,
          freeSpace: rootFolder.freeSpace
        };
      });

      if (includeNoChange) {
        values.unshift({
          key: 'noChange',
          value: 'No Change',
          isDisabled: true
        });
      }

      if (!values.length) {
        values.push({
          key: '',
          value: '',
          isDisabled: true,
          isHidden: true
        });
      }

      values.push({
        key: ADD_NEW_KEY,
        value: 'Add a new path'
      });

      return {
        values,
        isSaving: rootFolders.isSaving,
        saveError: rootFolders.saveError
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchAddRootFolder(path) {
      dispatch(addRootFolder({ path }));
    }
  };
}

class RootFolderSelectInputConnector extends Component {

  //
  // Lifecycle

  componentWillMount() {
    const {
      value,
      values,
      onChange
    } = this.props;

    if (value == null && values[0].key === '') {
      onChange({ name, value: '' });
    }
  }

  componentDidMount() {
    const {
      name,
      value,
      values,
      onChange
    } = this.props;

    if (!value || !_.some(values, (v) => v.hasOwnProperty(value)) || value === ADD_NEW_KEY) {
      const defaultValue = values[0];

      if (defaultValue.key === ADD_NEW_KEY) {
        onChange({ name, value: '' });
      } else {
        onChange({ name, value: defaultValue.key });
      }
    }
  }

  //
  // Listeners

  onNewRootFolderSelect = (path) => {
    this.props.dispatchAddRootFolder(path);
  }

  //
  // Render

  render() {
    const {
      dispatchAddRootFolder,
      ...otherProps
    } = this.props;

    return (
      <RootFolderSelectInput
        {...otherProps}
        onNewRootFolderSelect={this.onNewRootFolderSelect}
      />
    );
  }
}

RootFolderSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.string,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  includeNoChange: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired,
  dispatchAddRootFolder: PropTypes.func.isRequired
};

RootFolderSelectInputConnector.defaultProps = {
  includeNoChange: false
};

export default connect(createMapStateToProps, createMapDispatchToProps)(RootFolderSelectInputConnector);
