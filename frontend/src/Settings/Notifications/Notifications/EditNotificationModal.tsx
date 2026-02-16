import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditNotificationModalContent, {
  EditNotificationModalContentProps,
} from './EditNotificationModalContent';

interface EditNotificationModalProps extends EditNotificationModalContentProps {
  isOpen: boolean;
}

function EditNotificationModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditNotificationModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditNotificationModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditNotificationModal;
