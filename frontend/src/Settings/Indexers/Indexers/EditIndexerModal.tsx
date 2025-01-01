import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  cancelSaveIndexer,
  cancelTestIndexer,
} from 'Store/Actions/settingsActions';
import EditIndexerModalContent, {
  EditIndexerModalContentProps,
} from './EditIndexerModalContent';

const section = 'settings.indexers';

interface EditIndexerModalProps extends EditIndexerModalContentProps {
  isOpen: boolean;
}

function EditIndexerModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditIndexerModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section }));
    dispatch(cancelTestIndexer({ section }));
    dispatch(cancelSaveIndexer({ section }));

    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditIndexerModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditIndexerModal;
