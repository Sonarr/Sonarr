import React from 'react';
import Modal from 'Components/Modal/Modal';
import DeleteSeriesModalContent, {
  DeleteSeriesModalContentProps,
} from './DeleteSeriesModalContent';

interface DeleteSeriesModalProps extends DeleteSeriesModalContentProps {
  isOpen: boolean;
}

function DeleteSeriesModal(props: DeleteSeriesModalProps) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <DeleteSeriesModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default DeleteSeriesModal;
