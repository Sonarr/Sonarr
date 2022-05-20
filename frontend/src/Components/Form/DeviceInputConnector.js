import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearOptions, defaultState, fetchOptions } from 'Store/Actions/providerOptionActions';
import DeviceInput from './DeviceInput';

function createMapStateToProps() {
  return createSelector(
    (state, { value }) => value,
    (state) => state.providerOptions.devices || defaultState,
    (value, devices) => {

      return {
        ...devices,
        selectedDevices: value.map((valueDevice) => {
          // Disable equality ESLint rule so we don't need to worry about
          // a type mismatch between the value items and the device ID.
          // eslint-disable-next-line eqeqeq
          const device = devices.items.find((d) => d.id == valueDevice);

          if (device) {
            return {
              id: device.id,
              name: `${device.name} (${device.id})`
            };
          }

          return {
            id: valueDevice,
            name: `Unknown (${valueDevice})`
          };
        })
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchOptions: fetchOptions,
  dispatchClearOptions: clearOptions
};

class DeviceInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount = () => {
    this._populate();
  };

  componentWillUnmount = () => {
    this.props.dispatchClearOptions({ section: 'devices' });
  };

  //
  // Control

  _populate() {
    const {
      provider,
      providerData,
      dispatchFetchOptions
    } = this.props;

    dispatchFetchOptions({
      section: 'devices',
      action: 'getDevices',
      provider,
      providerData
    });
  }

  //
  // Listeners

  onRefreshPress = () => {
    this._populate();
  };

  //
  // Render

  render() {
    return (
      <DeviceInput
        {...this.props}
        onRefreshPress={this.onRefreshPress}
      />
    );
  }
}

DeviceInputConnector.propTypes = {
  provider: PropTypes.string.isRequired,
  providerData: PropTypes.object.isRequired,
  name: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  dispatchFetchOptions: PropTypes.func.isRequired,
  dispatchClearOptions: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(DeviceInputConnector);
