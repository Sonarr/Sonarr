import React from 'react';
import Modal from 'Components/Modal/Modal';
import Series from 'Series/Series';
import SelectSeriesModalContent from './SelectSeriesModalContent';

interface SelectSeriesModalProps {
  isOpen: boolean;
  modalTitle: string;
  onSeriesSelect(series: Series): void;
  onModalClose(): void;
}

function SelectSeriesModal(props: SelectSeriesModalProps) {
  const { isOpen, modalTitle, onSeriesSelect, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectSeriesModalContent
        modalTitle={modalTitle}
        onSeriesSelect={onSeriesSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectSeriesModal;
