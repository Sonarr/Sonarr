import React from 'react';
import Modal from 'Components/Modal/Modal';
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
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddNotificationModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default AddNotificationModal;
