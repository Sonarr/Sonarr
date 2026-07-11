import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
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
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <AddSpecificationModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default AddSpecificationModal;
