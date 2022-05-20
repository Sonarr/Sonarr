import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { cancelSaveNotification, cancelTestNotification } from 'Store/Actions/settingsActions';
import EditNotificationModal from './EditNotificationModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'settings.notifications';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges({ section }));
    },

    dispatchCancelTestNotification() {
      dispatch(cancelTestNotification({ section }));
    },

    dispatchCancelSaveNotification() {
      dispatch(cancelSaveNotification({ section }));
    }
  };
}

class EditNotificationModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges();
    this.props.dispatchCancelTestNotification();
    this.props.dispatchCancelSaveNotification();
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      dispatchCancelTestNotification,
      dispatchCancelSaveNotification,
      ...otherProps
    } = this.props;

    return (
      <EditNotificationModal
        {...otherProps}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditNotificationModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  dispatchCancelTestNotification: PropTypes.func.isRequired,
  dispatchCancelSaveNotification: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditNotificationModalConnector);
