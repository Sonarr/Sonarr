import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditImportListExclusionModal from './EditImportListExclusionModal';

function mapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  clearPendingChanges
};

class EditImportListExclusionModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'settings.importListExclusions' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <EditImportListExclusionModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditImportListExclusionModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(EditImportListExclusionModalConnector);
