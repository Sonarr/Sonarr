import React from 'react';
import Modal from 'Components/Modal/Modal';
import ReleaseType from 'InteractiveImport/ReleaseType';
import SelectReleaseTypeModalContent from './SelectReleaseTypeModalContent';

interface SelectQualityModalProps {
  isOpen: boolean;
  releaseType: ReleaseType;
  modalTitle: string;
  onReleaseTypeSelect(releaseType: ReleaseType): void;
  onModalClose(): void;
}

function SelectReleaseTypeModal(props: SelectQualityModalProps) {
  const { isOpen, releaseType, modalTitle, onReleaseTypeSelect, onModalClose } =
    props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectReleaseTypeModalContent
        releaseType={releaseType}
        modalTitle={modalTitle}
        onReleaseTypeSelect={onReleaseTypeSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectReleaseTypeModal;
