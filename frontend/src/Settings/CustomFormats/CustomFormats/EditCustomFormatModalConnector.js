import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditCustomFormatModal from './EditCustomFormatModal';

function mapStateToProps() {
  return {};
}

const mapDispatchToProps = {
  clearPendingChanges
};

class EditCustomFormatModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'settings.customFormats' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <EditCustomFormatModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditCustomFormatModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(EditCustomFormatModalConnector);
