import React from 'react';
import Modal from 'Components/Modal/Modal';
import ManageIndexersEditModalContent from './ManageIndexersEditModalContent';

interface ManageIndexersEditModalProps {
  isOpen: boolean;
  indexerIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

function ManageIndexersEditModal(props: ManageIndexersEditModalProps) {
  const { isOpen, indexerIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ManageIndexersEditModalContent
        indexerIds={indexerIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default ManageIndexersEditModal;
