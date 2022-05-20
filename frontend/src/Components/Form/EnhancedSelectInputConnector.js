import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearOptions, defaultState, fetchOptions } from 'Store/Actions/providerOptionActions';
import EnhancedSelectInput from './EnhancedSelectInput';

const importantFieldNames = [
  'baseUrl',
  'apiPath',
  'apiKey'
];

function getProviderDataKey(providerData) {
  if (!providerData || !providerData.fields) {
    return null;
  }

  const fields = providerData.fields
    .filter((f) => importantFieldNames.includes(f.name))
    .map((f) => f.value);

  return fields;
}

function getSelectOptions(items) {
  if (!items) {
    return [];
  }

  return items.map((option) => {
    return {
      key: option.value,
      value: option.name,
      hint: option.hint,
      parentKey: option.parentValue
    };
  });
}

function createMapStateToProps() {
  return createSelector(
    (state, { selectOptionsProviderAction }) => state.providerOptions[selectOptionsProviderAction] || defaultState,
    (options) => {
      if (options) {
        return {
          isFetching: options.isFetching,
          values: getSelectOptions(options.items)
        };
      }
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchOptions: fetchOptions,
  dispatchClearOptions: clearOptions
};

class EnhancedSelectInputConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      refetchRequired: false
    };
  }

  componentDidMount = () => {
    this._populate();
  };

  componentDidUpdate = (prevProps) => {
    const prevKey = getProviderDataKey(prevProps.providerData);
    const nextKey = getProviderDataKey(this.props.providerData);

    if (!_.isEqual(prevKey, nextKey)) {
      this.setState({ refetchRequired: true });
    }
  };

  componentWillUnmount = () => {
    this._cleanup();
  };

  //
  // Listeners

  onOpen = () => {
    if (this.state.refetchRequired) {
      this._populate();
    }
  };

  //
  // Control

  _populate() {
    const {
      provider,
      providerData,
      selectOptionsProviderAction,
      dispatchFetchOptions
    } = this.props;

    if (selectOptionsProviderAction) {
      this.setState({ refetchRequired: false });
      dispatchFetchOptions({
        section: selectOptionsProviderAction,
        action: selectOptionsProviderAction,
        provider,
        providerData
      });
    }
  }

  _cleanup() {
    const {
      selectOptionsProviderAction,
      dispatchClearOptions
    } = this.props;

    if (selectOptionsProviderAction) {
      dispatchClearOptions({ section: selectOptionsProviderAction });
    }
  }

  //
  // Render

  render() {
    return (
      <EnhancedSelectInput
        {...this.props}
        onOpen={this.onOpen}
      />
    );
  }
}

EnhancedSelectInputConnector.propTypes = {
  provider: PropTypes.string.isRequired,
  providerData: PropTypes.object.isRequired,
  name: PropTypes.string.isRequired,
  value: PropTypes.arrayOf(PropTypes.oneOfType([PropTypes.number, PropTypes.string])).isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  selectOptionsProviderAction: PropTypes.string,
  onChange: PropTypes.func.isRequired,
  isFetching: PropTypes.bool.isRequired,
  dispatchFetchOptions: PropTypes.func.isRequired,
  dispatchClearOptions: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EnhancedSelectInputConnector);
