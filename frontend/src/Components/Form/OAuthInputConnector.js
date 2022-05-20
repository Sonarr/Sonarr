import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { resetOAuth, startOAuth } from 'Store/Actions/oAuthActions';
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
  };

  //
  // Listeners

  onPress = () => {
    const {
      name,
      provider,
      providerData,
      section
    } = this.props;

    this.props.startOAuth({
      name,
      provider,
      providerData,
      section
    });
  };

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
  name: PropTypes.string.isRequired,
  result: PropTypes.object,
  provider: PropTypes.string.isRequired,
  providerData: PropTypes.object.isRequired,
  section: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  startOAuth: PropTypes.func.isRequired,
  resetOAuth: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(OAuthInputConnector);
