import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import MetadataSettingsModalContent, {
  MetadataSettingsModalContentProps,
} from './MetadataSettingsModalContent';

interface MetadataSettingsModalProps extends MetadataSettingsModalContentProps {
  isOpen: boolean;
}

function MetadataSettingsModal({
  isOpen,
  onModalClose,
}: MetadataSettingsModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <MetadataSettingsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default MetadataSettingsModal;
