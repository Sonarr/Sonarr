import React from 'react';
import Modal from 'Components/Modal/Modal';
import ManageImportListsModalContent from './ManageImportListsModalContent';

interface ManageImportListsModalProps {
  isOpen: boolean;
  onModalClose(): void;
}

function ManageImportListsModal(props: ManageImportListsModalProps) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ManageImportListsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default ManageImportListsModal;
