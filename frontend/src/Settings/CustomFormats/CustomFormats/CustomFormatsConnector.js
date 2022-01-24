import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cloneCustomFormat, deleteCustomFormat, fetchCustomFormats } from 'Store/Actions/settingsActions';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import sortByName from 'Utilities/Array/sortByName';
import CustomFormats from './CustomFormats';

function createMapStateToProps() {
  return createSelector(
    createSortedSectionSelector('settings.customFormats', sortByName),
    (customFormats) => customFormats
  );
}

const mapDispatchToProps = {
  dispatchFetchCustomFormats: fetchCustomFormats,
  dispatchDeleteCustomFormat: deleteCustomFormat,
  dispatchCloneCustomFormat: cloneCustomFormat
};

class CustomFormatsConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.dispatchFetchCustomFormats();
  }

  //
  // Listeners

  onConfirmDeleteCustomFormat = (id) => {
    this.props.dispatchDeleteCustomFormat({ id });
  };

  onCloneCustomFormatPress = (id) => {
    this.props.dispatchCloneCustomFormat({ id });
  };

  //
  // Render

  render() {
    return (
      <CustomFormats
        onConfirmDeleteCustomFormat={this.onConfirmDeleteCustomFormat}
        onCloneCustomFormatPress={this.onCloneCustomFormatPress}
        {...this.props}
      />
    );
  }
}

CustomFormatsConnector.propTypes = {
  dispatchFetchCustomFormats: PropTypes.func.isRequired,
  dispatchDeleteCustomFormat: PropTypes.func.isRequired,
  dispatchCloneCustomFormat: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(CustomFormatsConnector);
