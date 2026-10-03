import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import AddNotificationModalContent, {
  AddNotificationModalContentProps,
} from './AddNotificationModalContent';

interface AddNotificationModalProps extends AddNotificationModalContentProps {
  isOpen: boolean;
}

function AddNotificationModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddNotificationModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <AddNotificationModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default AddNotificationModal;
