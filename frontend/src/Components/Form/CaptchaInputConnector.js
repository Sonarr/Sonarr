import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { getCaptchaCookie, refreshCaptcha, resetCaptcha } from 'Store/Actions/captchaActions';
import CaptchaInput from './CaptchaInput';

function createMapStateToProps() {
  return createSelector(
    (state) => state.captcha,
    (captcha) => {
      return captcha;
    }
  );
}

const mapDispatchToProps = {
  refreshCaptcha,
  getCaptchaCookie,
  resetCaptcha
};

class CaptchaInputConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps) {
    const {
      name,
      token,
      onChange
    } = this.props;

    if (token && token !== prevProps.token) {
      onChange({ name, value: token });
    }
  }

  componentWillUnmount = () => {
    this.props.resetCaptcha();
  };

  //
  // Listeners

  onRefreshPress = () => {
    const {
      provider,
      providerData
    } = this.props;

    this.props.refreshCaptcha({ provider, providerData });
  };

  onCaptchaChange = (captchaResponse) => {
    // If the captcha has expired `captchaResponse` will be null.
    // In the event it's null don't try to get the captchaCookie.
    // TODO: Should we clear the cookie? or reset the captcha?

    if (!captchaResponse) {
      return;
    }

    const {
      provider,
      providerData
    } = this.props;

    this.props.getCaptchaCookie({ provider, providerData, captchaResponse });
  };

  //
  // Render

  render() {
    return (
      <CaptchaInput
        {...this.props}
        onRefreshPress={this.onRefreshPress}
        onCaptchaChange={this.onCaptchaChange}
      />
    );
  }
}

CaptchaInputConnector.propTypes = {
  provider: PropTypes.string.isRequired,
  providerData: PropTypes.object.isRequired,
  name: PropTypes.string.isRequired,
  token: PropTypes.string,
  onChange: PropTypes.func.isRequired,
  refreshCaptcha: PropTypes.func.isRequired,
  getCaptchaCookie: PropTypes.func.isRequired,
  resetCaptcha: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(CaptchaInputConnector);
