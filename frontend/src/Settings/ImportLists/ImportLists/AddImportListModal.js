import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddImportListModalContentConnector from './AddImportListModalContentConnector';

function AddImportListModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <AddImportListModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

AddImportListModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddImportListModal;
