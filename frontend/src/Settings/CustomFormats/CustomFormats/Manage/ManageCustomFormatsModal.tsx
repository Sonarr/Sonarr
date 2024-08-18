import React from 'react';
import Modal from 'Components/Modal/Modal';
import ManageCustomFormatsModalContent from './ManageCustomFormatsModalContent';

interface ManageCustomFormatsModalProps {
  isOpen: boolean;
  onModalClose(): void;
}

function ManageCustomFormatsModal(props: ManageCustomFormatsModalProps) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ManageCustomFormatsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default ManageCustomFormatsModal;
