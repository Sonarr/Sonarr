import React from 'react';
import Modal from 'Components/Modal/Modal';
import ManageDownloadClientsModalContent from './ManageDownloadClientsModalContent';

interface ManageDownloadClientsModalProps {
  isOpen: boolean;
  onModalClose(): void;
}

function ManageDownloadClientsModal(props: ManageDownloadClientsModalProps) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ManageDownloadClientsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default ManageDownloadClientsModal;
