import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import ExportCustomFormatModalContentConnector from './ExportCustomFormatModalContentConnector';

class ExportCustomFormatModal extends Component {

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
  };

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
        size={sizes.LARGE}
        onModalClose={onModalClose}
      >
        <ExportCustomFormatModalContentConnector
          {...otherProps}
          onContentHeightChange={this.onContentHeightChange}
          onModalClose={onModalClose}
        />
      </Modal>
    );
  }
}

ExportCustomFormatModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ExportCustomFormatModal;
