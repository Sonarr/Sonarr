import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import AuthenticationRequiredModalContent from './AuthenticationRequiredModalContent';

function onModalClose() {
  // No-op
}

interface AuthenticationRequiredModalProps {
  isOpen: boolean;
}

export default function AuthenticationRequiredModal({
  isOpen,
}: AuthenticationRequiredModalProps) {
  return (
    <Modal
      size={sizes.MEDIUM}
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <AuthenticationRequiredModalContent />
    </Modal>
  );
}
