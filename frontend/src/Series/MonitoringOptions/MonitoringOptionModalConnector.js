import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import MonitoringOptionsModal from './EditSeriesModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class MonitoringOptionsModalConnector extends Component {

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
      <MonitoringOptionsModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

MonitoringOptionsModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(MonitoringOptionsModalConnector);
