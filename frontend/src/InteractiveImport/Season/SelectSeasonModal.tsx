import React from 'react';
import Modal from 'Components/Modal/Modal';
import SelectSeasonModalContent from './SelectSeasonModalContent';

interface SelectSeasonModalProps {
  isOpen: boolean;
  modalTitle: string;
  seriesId?: number;
  onSeasonSelect(seasonNumber: number): void;
  onModalClose(): void;
}

function SelectSeasonModal(props: SelectSeasonModalProps) {
  const { isOpen, modalTitle, seriesId, onSeasonSelect, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectSeasonModalContent
        modalTitle={modalTitle}
        seriesId={seriesId}
        onSeasonSelect={onSeasonSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectSeasonModal;
