import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditSeriesModalContent from './EditSeriesModalContent';

interface EditSeriesModalProps {
  isOpen: boolean;
  seriesIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

function EditSeriesModal(props: EditSeriesModalProps) {
  const { isOpen, seriesIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditSeriesModalContent
        seriesIds={seriesIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditSeriesModal;
