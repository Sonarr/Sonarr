import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { toggleAdvancedSettings } from 'Store/Actions/settingsActions';
import SettingsToolbar from './SettingsToolbar';

function mapStateToProps(state) {
  return {
    advancedSettings: state.settings.advancedSettings
  };
}

const mapDispatchToProps = {
  toggleAdvancedSettings
};

class SettingsToolbarConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      nextLocation: null,
      nextLocationAction: null,
      confirmed: false
    };

    this._unblock = null;
  }

  componentDidMount() {
    this._unblock = this.props.history.block(this.routerWillLeave);
  }

  componentWillUnmount() {
    if (this._unblock) {
      this._unblock();
    }
  }

  //
  // Control

  routerWillLeave = (nextLocation, nextLocationAction) => {
    if (this.state.confirmed) {
      this.setState({
        nextLocation: null,
        nextLocationAction: null,
        confirmed: false
      });

      return true;
    }

    if (this.props.hasPendingChanges ) {
      this.setState({
        nextLocation,
        nextLocationAction
      });

      return false;
    }

    return true;
  }

  //
  // Listeners

  onAdvancedSettingsPress = () => {
    this.props.toggleAdvancedSettings();
  }

  onConfirmNavigation = () => {
    const {
      nextLocation,
      nextLocationAction
    } = this.state;

    const history = this.props.history;

    const path = `${nextLocation.pathname}${nextLocation.search}`;

    this.setState({
      confirmed: true
    }, () => {
      if (nextLocationAction === 'PUSH') {
        history.push(path);
      } else {
        // Unfortunately back and forward both use POP,
        // which means we don't actually know which direction
        // the user wanted to go, assuming back.

        history.goBack();
      }
    });
  }

  onCancelNavigation = () => {
    this.setState({
      nextLocation: null,
      nextLocationAction: null,
      confirmed: false
    });
  }

  //
  // Render

  render() {
    const hasPendingLocation = this.state.nextLocation !== null;

    return (
      <SettingsToolbar
        hasPendingLocation={hasPendingLocation}
        onSavePress={this.props.onSavePress}
        onAdvancedSettingsPress={this.onAdvancedSettingsPress}
        onConfirmNavigation={this.onConfirmNavigation}
        onCancelNavigation={this.onCancelNavigation}
        {...this.props}
      />
    );
  }
}

const historyShape = {
  block: PropTypes.func.isRequired,
  goBack: PropTypes.func.isRequired,
  push: PropTypes.func.isRequired
};

SettingsToolbarConnector.propTypes = {
  hasPendingChanges: PropTypes.bool.isRequired,
  history: PropTypes.shape(historyShape).isRequired,
  onSavePress: PropTypes.func,
  toggleAdvancedSettings: PropTypes.func.isRequired
};

SettingsToolbarConnector.defaultProps = {
  hasPendingChanges: false
};

export default withRouter(connect(mapStateToProps, mapDispatchToProps)(SettingsToolbarConnector));
