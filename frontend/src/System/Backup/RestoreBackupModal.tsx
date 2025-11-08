import React from 'react';
import Modal from 'Components/Modal/Modal';
import RestoreBackupModalContent, {
  RestoreBackupModalContentProps,
} from './RestoreBackupModalContent';

interface RestoreBackupModalProps extends RestoreBackupModalContentProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function RestoreBackupModal({
  isOpen,
  onModalClose,
  ...otherProps
}: RestoreBackupModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <RestoreBackupModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default RestoreBackupModal;
