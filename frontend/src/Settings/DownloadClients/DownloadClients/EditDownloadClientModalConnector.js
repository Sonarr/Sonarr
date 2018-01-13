import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import { cancelTestDownloadClient, cancelSaveDownloadClient } from 'Store/Actions/settingsActions';
import EditDownloadClientModal from './EditDownloadClientModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'settings.downloadClients';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges({ section }));
    },

    dispatchCancelTestDownloadClient() {
      dispatch(cancelTestDownloadClient({ section }));
    },

    dispatchCancelSaveDownloadClient() {
      dispatch(cancelSaveDownloadClient({ section }));
    }
  };
}

class EditDownloadClientModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges();
    this.props.dispatchCancelTestDownloadClient();
    this.props.dispatchCancelSaveDownloadClient();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      dispatchCancelTestDownloadClient,
      dispatchCancelSaveDownloadClient,
      ...otherProps
    } = this.props;

    return (
      <EditDownloadClientModal
        {...otherProps}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditDownloadClientModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  dispatchCancelTestDownloadClient: PropTypes.func.isRequired,
  dispatchCancelSaveDownloadClient: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditDownloadClientModalConnector);
