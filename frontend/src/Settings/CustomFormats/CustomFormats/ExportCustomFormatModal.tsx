import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import ExportCustomFormatModalContent, {
  ExportCustomFormatModalContentProps,
} from './ExportCustomFormatModalContent';

interface ExportCustomFormatModalProps
  extends ExportCustomFormatModalContentProps {
  isOpen: boolean;
}

function ExportCustomFormatModal({
  isOpen,
  onModalClose,
  ...otherProps
}: ExportCustomFormatModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.LARGE} onModalClose={onModalClose}>
      <ExportCustomFormatModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default ExportCustomFormatModal;
