import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditImportListModalContent, {
  EditImportListModalContentProps,
} from './EditImportListModalContent';

interface EditImportListModalProps extends EditImportListModalContentProps {
  isOpen: boolean;
}

function EditImportListModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditImportListModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditImportListModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default EditImportListModal;
