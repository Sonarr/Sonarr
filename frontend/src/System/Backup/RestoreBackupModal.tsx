import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { clearRestoreBackup } from 'Store/Actions/systemActions';
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
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearRestoreBackup());
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleModalClose}>
      <RestoreBackupModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default RestoreBackupModal;
