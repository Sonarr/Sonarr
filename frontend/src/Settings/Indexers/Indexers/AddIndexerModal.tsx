import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddIndexerModalContent from './AddIndexerModalContent';

interface AddIndexerModalProps {
  isOpen: boolean;
  onIndexerSelect: () => void;
  onModalClose: () => void;
}

function AddIndexerModal({
  isOpen,
  onIndexerSelect,
  onModalClose,
}: AddIndexerModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddIndexerModalContent
        onIndexerSelect={onIndexerSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default AddIndexerModal;
