import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditQualityProfileModal from './EditQualityProfileModal';

function mapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  clearPendingChanges
};

class EditQualityProfileModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'settings.qualityProfiles' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditQualityProfileModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditQualityProfileModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(EditQualityProfileModalConnector);
