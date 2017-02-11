import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditLanguageProfileModal from './EditLanguageProfileModal';

function mapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  clearPendingChanges
};

class EditLanguageProfileModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'languageProfiles' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditLanguageProfileModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditLanguageProfileModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(EditLanguageProfileModalConnector);
