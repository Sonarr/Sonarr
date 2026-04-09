import React from 'react';
import Modal from 'Components/Modal/Modal';
import { AutoTaggingSpecification } from '../useAutoTaggings';
import AddSpecificationModalContent from './AddSpecificationModalContent';

interface AddSpecificationModalProps {
  isOpen: boolean;
  onModalClose: (selectedSpec?: AutoTaggingSpecification) => void;
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
