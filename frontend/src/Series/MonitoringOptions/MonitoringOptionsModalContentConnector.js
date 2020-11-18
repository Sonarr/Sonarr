import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { updateSeriesMonitor } from 'Store/Actions/seriesActions';
import MonitoringOptionsModalContent from './MonitoringOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.series,
    (seriesState) => {
      const {
        isSaving,
        saveError
      } = seriesState;

      return {
        isSaving,
        saveError
      };
    }
  );
}

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

  onSavePress = ({ monitor }) => {
    this.props.dispatchUpdateMonitoringOptions({
      id: this.props.seriesId,
      monitor
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
  onSavePress: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MonitoringOptionsModalContentConnector);
