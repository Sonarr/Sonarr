import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddSpecificationModalContent, {
  AddSpecificationModalContentProps,
} from './AddSpecificationModalContent';

interface AddSpecificationModalProps extends AddSpecificationModalContentProps {
  isOpen: boolean;
}

function AddSpecificationModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddSpecificationModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddSpecificationModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default AddSpecificationModal;
