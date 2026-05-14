import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import ImportCustomFormatModalContent from './ImportCustomFormatModalContent';
import { CustomFormat } from './useCustomFormats';

interface ImportCustomFormatModalProps {
  isOpen: boolean;
  onImport: (customFormat: CustomFormat) => void;
  onModalClose: () => void;
}

function ImportCustomFormatModal({
  isOpen,
  onImport,
  onModalClose,
}: ImportCustomFormatModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.LARGE} onModalClose={onModalClose}>
      <ImportCustomFormatModalContent
        onImport={onImport}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default ImportCustomFormatModal;
