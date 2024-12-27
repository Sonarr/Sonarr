import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddSpecificationModalContent from './AddSpecificationModalContent';

interface AddSpecificationModalProps {
  isOpen: boolean;
  onModalClose: (options?: { specificationSelected: boolean }) => void;
}

function AddSpecificationModal({
  isOpen,
  onModalClose,
}: AddSpecificationModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddSpecificationModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default AddSpecificationModal;
