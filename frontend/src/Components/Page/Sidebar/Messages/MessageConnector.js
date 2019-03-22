import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { hideMessage } from 'Store/Actions/appActions';
import Message from './Message';

const mapDispatchToProps = {
  hideMessage
};

class MessageConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._hideTimeoutId = null;
    this.scheduleHideMessage(props.hideAfter);
  }

  componentDidUpdate() {
    this.scheduleHideMessage(this.props.hideAfter);
  }

  //
  // Control

  scheduleHideMessage = (hideAfter) => {
    if (this._hideTimeoutId) {
      clearTimeout(this._hideTimeoutId);
    }

    if (hideAfter) {
      this._hideTimeoutId = setTimeout(this.hideMessage, hideAfter * 1000);
    }
  }

  hideMessage = () => {
    this.props.hideMessage({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <Message
        {...this.props}
      />
    );
  }
}

MessageConnector.propTypes = {
  id: PropTypes.oneOfType([PropTypes.number, PropTypes.string]).isRequired,
  hideAfter: PropTypes.number.isRequired,
  hideMessage: PropTypes.func.isRequired
};

MessageConnector.defaultProps = {
  // Hide messages after 60 seconds if there is no activity
  // hideAfter: 60
};

export default connect(undefined, mapDispatchToProps)(MessageConnector);
