import React from 'react';
import Modal from 'Components/Modal/Modal';
import ManageDownloadClientsEditModalContent from './ManageDownloadClientsEditModalContent';

interface ManageDownloadClientsEditModalProps {
  isOpen: boolean;
  downloadClientIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

function ManageDownloadClientsEditModal(
  props: ManageDownloadClientsEditModalProps
) {
  const { isOpen, downloadClientIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ManageDownloadClientsEditModalContent
        downloadClientIds={downloadClientIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default ManageDownloadClientsEditModal;
