import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddSpecificationModalContent from './AddSpecificationModalContent';

function AddSpecificationModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <AddSpecificationModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

AddSpecificationModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddSpecificationModal;
