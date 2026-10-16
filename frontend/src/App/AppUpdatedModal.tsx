import React, { useCallback } from 'react';
import Modal from 'Components/Modal/Modal';
import AppUpdatedModalContent from './AppUpdatedModalContent';

interface AppUpdatedModalProps {
  isOpen: boolean;
}

function AppUpdatedModal(props: AppUpdatedModalProps) {
  const { isOpen } = props;

  const handleModalClose = useCallback(() => {
    location.reload();
  }, []);

  return (
    <Modal
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      onModalClose={handleModalClose}
    >
      <AppUpdatedModalContent onModalClose={handleModalClose} />
    </Modal>
  );
}

export default AppUpdatedModal;
