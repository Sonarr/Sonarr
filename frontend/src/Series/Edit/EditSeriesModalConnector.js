import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditSeriesModal from './EditSeriesModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditSeriesModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'series' });
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <EditSeriesModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditSeriesModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditSeriesModalConnector);
