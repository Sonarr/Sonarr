import React from 'react';
import Modal from 'Components/Modal/Modal';
import ParseModalContent from './ParseModalContent';

interface ParseModalProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function ParseModal(props: ParseModalProps) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ParseModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default ParseModal;
