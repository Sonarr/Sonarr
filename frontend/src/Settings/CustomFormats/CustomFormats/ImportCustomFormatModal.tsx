import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import ImportCustomFormatModalContent from './ImportCustomFormatModalContent';

interface ImportCustomFormatModalProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function ImportCustomFormatModal({
  isOpen,
  onModalClose,
}: ImportCustomFormatModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.LARGE} onModalClose={onModalClose}>
      <ImportCustomFormatModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default ImportCustomFormatModal;
