import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import DeleteSeriesModalContent, {
  DeleteSeriesModalContentProps,
} from './DeleteSeriesModalContent';

interface DeleteSeriesModalProps extends DeleteSeriesModalContentProps {
  isOpen: boolean;
}

function DeleteSeriesModal({
  isOpen,
  onModalClose,
  ...otherProps
}: DeleteSeriesModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <DeleteSeriesModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default DeleteSeriesModal;
