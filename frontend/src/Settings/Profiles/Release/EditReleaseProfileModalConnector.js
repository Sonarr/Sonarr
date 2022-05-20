import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditReleaseProfileModal from './EditReleaseProfileModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditReleaseProfileModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'settings.releaseProfiles' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <EditReleaseProfileModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditReleaseProfileModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(null, mapDispatchToProps)(EditReleaseProfileModalConnector);
