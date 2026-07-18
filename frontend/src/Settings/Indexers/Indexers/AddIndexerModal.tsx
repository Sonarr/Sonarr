import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import AddIndexerModalContent, {
  AddIndexerModalContentProps,
} from './AddIndexerModalContent';

interface AddIndexerModalProps extends AddIndexerModalContentProps {
  isOpen: boolean;
}

function AddIndexerModal({
  isOpen,
  onIndexerSelect,
  onModalClose,
}: AddIndexerModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <AddIndexerModalContent
        onIndexerSelect={onIndexerSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default AddIndexerModal;
