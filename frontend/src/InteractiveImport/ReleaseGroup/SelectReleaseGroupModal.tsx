import React from 'react';
import Modal from 'Components/Modal/Modal';
import SelectReleaseGroupModalContent from './SelectReleaseGroupModalContent';

interface SelectReleaseGroupModalProps {
  isOpen: boolean;
  releaseGroup: string;
  modalTitle: string;
  onReleaseGroupSelect(releaseGroup: string): void;
  onModalClose(): void;
}

function SelectReleaseGroupModal(props: SelectReleaseGroupModalProps) {
  const {
    isOpen,
    releaseGroup,
    modalTitle,
    onReleaseGroupSelect,
    onModalClose,
  } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectReleaseGroupModalContent
        releaseGroup={releaseGroup}
        modalTitle={modalTitle}
        onReleaseGroupSelect={onReleaseGroupSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectReleaseGroupModal;
