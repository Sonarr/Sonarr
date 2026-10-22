import React from 'react';
import Modal from 'Components/Modal/Modal';
import SelectSeasonModalContent from './SelectSeasonModalContent';

interface SelectSeasonModalProps {
  isOpen: boolean;
  modalTitle: string;
  seriesId?: number;
  selectedSeasonNumbers?: number[];
  onSeasonSelect?(seasonNumber: number): void;
  onSeasonsSelect?(seasonNumbers: number[]): void;
  onModalClose(): void;
}

function SelectSeasonModal(props: SelectSeasonModalProps) {
  const {
    isOpen,
    modalTitle,
    seriesId,
    selectedSeasonNumbers,
    onSeasonSelect,
    onSeasonsSelect,
    onModalClose,
  } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectSeasonModalContent
        modalTitle={modalTitle}
        seriesId={seriesId}
        selectedSeasonNumbers={selectedSeasonNumbers}
        onSeasonSelect={onSeasonSelect}
        onSeasonsSelect={onSeasonsSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectSeasonModal;
