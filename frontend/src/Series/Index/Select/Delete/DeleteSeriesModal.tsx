import React from 'react';
import Modal from 'Components/Modal/Modal';
import DeleteSeriesModalContent, {
  DeleteSeriesModalContentProps,
} from './DeleteSeriesModalContent';

interface DeleteSeriesModalProps extends DeleteSeriesModalContentProps {
  isOpen: boolean;
}

function DeleteSeriesModal({
  isOpen,
  onDeletePress,
  onModalClose,
}: DeleteSeriesModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <DeleteSeriesModalContent
        onDeletePress={onDeletePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default DeleteSeriesModal;
