import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditSpecificationModalContentConnector from './EditSpecificationModalContentConnector';

function EditSpecificationModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      size={sizes.MEDIUM}
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditSpecificationModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditSpecificationModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditSpecificationModal;
