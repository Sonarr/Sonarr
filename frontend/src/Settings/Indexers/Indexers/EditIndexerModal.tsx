import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditIndexerModalContent, {
  EditIndexerModalContentProps,
} from './EditIndexerModalContent';

interface EditIndexerModalProps extends EditIndexerModalContentProps {
  isOpen: boolean;
}

function EditIndexerModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditIndexerModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditIndexerModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default EditIndexerModal;
