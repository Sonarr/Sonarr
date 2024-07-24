import React, { useCallback } from 'react';
import Modal from 'Components/Modal/Modal';
import AppUpdatedModalContent from './AppUpdatedModalContent';

interface AppUpdatedModalProps {
  isOpen: boolean;
  onModalClose: (...args: unknown[]) => unknown;
}

function AppUpdatedModal(props: AppUpdatedModalProps) {
  const { isOpen, onModalClose } = props;

  const handleModalClose = useCallback(() => {
    location.reload();
  }, []);

  return (
    <Modal
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <AppUpdatedModalContent onModalClose={handleModalClose} />
    </Modal>
  );
}

export default AppUpdatedModal;
