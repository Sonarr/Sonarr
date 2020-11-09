import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { updateSeriesMonitor } from 'Store/Actions/seriesActions';
import MonitoringOptionsModalContent from './MonitoringOptionsModalContent';

const mapDispatchToProps = {
  dispatchUpdateMonitoringOptions: updateSeriesMonitor
};

class MonitoringOptionsModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose(true);
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({ name, value });
  }

  onSavePress = (changes) => {
    this.props.dispatchUpdateMonitoringOptions({
      id: this.props.seriesId,
      monitor: changes.monitor
    });
  }

  //
  // Render

  render() {
    return (
      <MonitoringOptionsModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
      />
    );
  }
}

MonitoringOptionsModalContentConnector.propTypes = {
  seriesId: PropTypes.number.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchUpdateMonitoringOptions: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func,
  monitor: PropTypes.string
};

export default connect(undefined, mapDispatchToProps)(MonitoringOptionsModalContentConnector);
