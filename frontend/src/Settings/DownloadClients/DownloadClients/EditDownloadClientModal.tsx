import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  cancelSaveDownloadClient,
  cancelTestDownloadClient,
} from 'Store/Actions/settingsActions';
import EditDownloadClientModalContent, {
  EditDownloadClientModalContentProps,
} from './EditDownloadClientModalContent';

const section = 'settings.downloadClients';

interface EditDownloadClientModalProps
  extends EditDownloadClientModalContentProps {
  isOpen: boolean;
}

function EditDownloadClientModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditDownloadClientModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section }));
    dispatch(cancelTestDownloadClient({ section }));
    dispatch(cancelSaveDownloadClient({ section }));

    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditDownloadClientModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditDownloadClientModal;
