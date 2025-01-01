import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddImportListModalContent, {
  AddImportListModalContentProps,
} from './AddImportListModalContent';

interface AddImportListModalProps extends AddImportListModalContentProps {
  isOpen: boolean;
}

function AddImportListModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddImportListModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddImportListModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default AddImportListModal;
