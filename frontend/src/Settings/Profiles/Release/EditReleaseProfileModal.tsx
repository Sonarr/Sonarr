import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditReleaseProfileModalContent from './EditReleaseProfileModalContent';

interface EditReleaseProfileModalProps {
  id?: number;
  isOpen: boolean;
  onModalClose: () => void;
  onDeleteReleaseProfilePress?: () => void;
}

function EditReleaseProfileModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditReleaseProfileModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditReleaseProfileModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditReleaseProfileModal;
