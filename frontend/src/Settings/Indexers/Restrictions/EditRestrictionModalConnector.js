import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditRestrictionModal from './EditRestrictionModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditRestrictionModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'restrictions' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditRestrictionModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditRestrictionModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(null, mapDispatchToProps)(EditRestrictionModalConnector);
