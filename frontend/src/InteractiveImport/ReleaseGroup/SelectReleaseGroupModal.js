import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import SelectReleaseGroupModalContentConnector from './SelectReleaseGroupModalContentConnector';

class SelectReleaseGroupModal extends Component {

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
        isOpen={isOpen}
        onModalClose={onModalClose}
      >
        <SelectReleaseGroupModalContentConnector
          {...otherProps}
          onModalClose={onModalClose}
        />
      </Modal>
    );
  }
}

SelectReleaseGroupModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectReleaseGroupModal;
