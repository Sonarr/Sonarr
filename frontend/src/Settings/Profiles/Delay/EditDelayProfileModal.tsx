import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditDelayProfileModalContent, {
  EditDelayProfileModalContentProps,
} from './EditDelayProfileModalContent';

interface EditDelayProfileModalProps extends EditDelayProfileModalContentProps {
  isOpen: boolean;
}

function EditDelayProfileModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditDelayProfileModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditDelayProfileModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditDelayProfileModal;
