import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { sizes } from 'Helpers/Props';
import Modal from 'Components/Modal/Modal';
import EditQualityProfileModalContentConnector from './EditQualityProfileModalContentConnector';

class EditQualityProfileModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      height: 'auto'
    };
  }

  //
  // Listeners

  onContentHeightChange = (height) => {
    if (this.state.height === 'auto' || height > this.state.height) {
      this.setState({ height });
    }
  }

  //
  // Render

  render() {
    const {
      isOpen,
      onModalClose,
      ...otherProps
    } = this.props;

    return (
      <Modal
        style={{ height: `${this.state.height}px` }}
        isOpen={isOpen}
        size={sizes.EXTRA_LARGE}
        onModalClose={onModalClose}
      >
        <EditQualityProfileModalContentConnector
          {...otherProps}
          onContentHeightChange={this.onContentHeightChange}
          onModalClose={onModalClose}
        />
      </Modal>
    );
  }
}

EditQualityProfileModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditQualityProfileModal;
