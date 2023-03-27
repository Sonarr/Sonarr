import React from 'react';
import Modal from 'Components/Modal/Modal';
import { QualityModel } from 'Quality/Quality';
import SelectQualityModalContent from './SelectQualityModalContent';

interface SelectQualityModalProps {
  isOpen: boolean;
  qualityId: number;
  proper: boolean;
  real: boolean;
  modalTitle: string;
  onQualitySelect(quality: QualityModel): void;
  onModalClose(): void;
}

function SelectQualityModal(props: SelectQualityModalProps) {
  const {
    isOpen,
    qualityId,
    proper,
    real,
    modalTitle,
    onQualitySelect,
    onModalClose,
  } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectQualityModalContent
        qualityId={qualityId}
        proper={proper}
        real={real}
        modalTitle={modalTitle}
        onQualitySelect={onQualitySelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectQualityModal;
