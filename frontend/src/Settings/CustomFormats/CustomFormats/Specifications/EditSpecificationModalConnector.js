import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditSpecificationModal from './EditSpecificationModal';

function createMapDispatchToProps(dispatch, props) {
  const section = 'settings.customFormatSpecifications';

  return {
    dispatchClearPendingChanges() {
      dispatch(clearPendingChanges({ section }));
    }
  };
}

class EditSpecificationModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.dispatchClearPendingChanges();
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    const {
      dispatchClearPendingChanges,
      ...otherProps
    } = this.props;

    return (
      <EditSpecificationModal
        {...otherProps}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditSpecificationModalConnector.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(EditSpecificationModalConnector);
