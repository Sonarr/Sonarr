import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchOptions, clearOptions } from 'Store/Actions/providerOptionActions';
import FileSelectInput from './FileSelectInput';

function createMapStateToProps() {
  return createSelector(
    (state) => state.providerOptions,
    (files) => {
      return files;
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchOptions: fetchOptions,
  dispatchClearOptions: clearOptions
};

class FileSelectInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    this._populate();
  }

  componentWillUnmount = () => {
    this.props.dispatchClearOptions();
  }

  //
  // Control

  _populate() {
    const {
      provider,
      providerData,
      dispatchFetchOptions
    } = this.props;

    dispatchFetchOptions({
      action: 'getFiles',
      provider,
      providerData
    });
  }

  //
  // Listeners

  onRefreshPress = () => {
    this._populate();
  }

  //
  // Render

  render() {
    return (
      <FileSelectInput
        {...this.props}
        onRefreshPress={this.onRefreshPress}
      />
    );
  }
}

FileSelectInputConnector.propTypes = {
  provider: PropTypes.string.isRequired,
  providerData: PropTypes.object.isRequired,
  name: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  dispatchFetchOptions: PropTypes.func.isRequired,
  dispatchClearOptions: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(FileSelectInputConnector);
