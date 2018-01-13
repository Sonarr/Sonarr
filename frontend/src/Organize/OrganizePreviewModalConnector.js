import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearOrganizePreview } from 'Store/Actions/organizePreviewActions';
import OrganizePreviewModal from './OrganizePreviewModal';

const mapDispatchToProps = {
  clearOrganizePreview
};

class OrganizePreviewModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearOrganizePreview();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <OrganizePreviewModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

OrganizePreviewModalConnector.propTypes = {
  clearOrganizePreview: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(OrganizePreviewModalConnector);
