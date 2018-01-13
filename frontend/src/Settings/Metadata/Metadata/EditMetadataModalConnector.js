import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditMetadataModal from './EditMetadataModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'settings.metadata';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges({ section }));
    }
  };
}

class EditMetadataModalConnector extends Component {
  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges({ section: 'metadata' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditMetadataModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditMetadataModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditMetadataModalConnector);
