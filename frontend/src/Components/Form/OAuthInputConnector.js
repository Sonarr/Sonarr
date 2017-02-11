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
      accessToken,
      accessTokenSecret,
      onChange
    } = this.props;

    if (accessToken &&
        accessToken !== prevProps.accessToken &&
        accessTokenSecret &&
        accessTokenSecret !== prevProps.accessTokenSecret) {
      onChange({ name: 'AccessToken', value: accessToken });
      onChange({ name: 'AccessTokenSecret', value: accessTokenSecret });
    }
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
  accessToken: PropTypes.string,
  accessTokenSecret: PropTypes.string,
  provider: PropTypes.string.isRequired,
  providerData: PropTypes.object.isRequired,
  onChange: PropTypes.func.isRequired,
  startOAuth: PropTypes.func.isRequired,
  resetOAuth: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(OAuthInputConnector);
