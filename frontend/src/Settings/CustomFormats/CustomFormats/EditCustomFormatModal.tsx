import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditCustomFormatModalContent, {
  EditCustomFormatModalContentProps,
} from './EditCustomFormatModalContent';

interface EditCustomFormatModalProps extends EditCustomFormatModalContentProps {
  isOpen: boolean;
}

function EditCustomFormatModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditCustomFormatModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.LARGE} onModalClose={onModalClose}>
      <EditCustomFormatModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditCustomFormatModal;
