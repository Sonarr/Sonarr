import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import ResetQualityDefinitionsModalContent from './ResetQualityDefinitionsModalContent';

interface ResetQualityDefinitionsModalProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function ResetQualityDefinitionsModal({
  isOpen,
  onModalClose,
}: ResetQualityDefinitionsModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <ResetQualityDefinitionsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default ResetQualityDefinitionsModal;
