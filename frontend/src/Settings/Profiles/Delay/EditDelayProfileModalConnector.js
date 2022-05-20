import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditDelayProfileModal from './EditDelayProfileModal';

function mapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  clearPendingChanges
};

class EditDelayProfileModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'settings.delayProfiles' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <EditDelayProfileModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditDelayProfileModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(EditDelayProfileModalConnector);
