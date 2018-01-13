import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { startOAuth, resetOAuth } from 'Store/Actions/oAuthActions';
import OAuthInput from './OAuthInput';

function createMapStateToProps() {
  return createSelector(
    (state) => state.oAuth,
    (oAuth) => {
      return oAuth;
    }
  );
}

const mapDispatchToProps = {
  startOAuth,
  resetOAuth
};

class OAuthInputConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps) {
    const {
      result,
      onChange
    } = this.props;

    if (!result || result === prevProps.result) {
      return;
    }

    Object.keys(result).forEach((key) => {
      onChange({ name: key, value: result[key] });
    });
  }

  componentWillUnmount = () => {
    this.props.resetOAuth();
  }

  //
  // Listeners

  onPress = () => {
    const {
      provider,
      providerData
    } = this.props;

    this.props.startOAuth({ provider, providerData });
  }

  //
  // Render

  render() {
    return (
      <OAuthInput
        {...this.props}
        onPress={this.onPress}
      />
    );
  }
}

OAuthInputConnector.propTypes = {
  result: PropTypes.object,
  provider: PropTypes.string.isRequired,
  providerData: PropTypes.object.isRequired,
  onChange: PropTypes.func.isRequired,
  startOAuth: PropTypes.func.isRequired,
  resetOAuth: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(OAuthInputConnector);
