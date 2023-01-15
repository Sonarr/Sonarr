import React from 'react';
import Modal from 'Components/Modal/Modal';
import DeleteSeriesModalContent from './DeleteSeriesModalContent';

interface DeleteSeriesModalProps {
  isOpen: boolean;
  seriesIds: number[];
  onModalClose(): void;
}

function DeleteSeriesModal(props: DeleteSeriesModalProps) {
  const { isOpen, seriesIds, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <DeleteSeriesModalContent
        seriesIds={seriesIds}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default DeleteSeriesModal;
