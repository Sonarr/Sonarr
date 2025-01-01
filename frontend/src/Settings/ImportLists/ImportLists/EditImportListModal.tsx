import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  cancelSaveImportList,
  cancelTestImportList,
} from 'Store/Actions/settingsActions';
import EditImportListModalContent, {
  EditImportListModalContentProps,
} from './EditImportListModalContent';

const section = 'settings.importLists';

interface EditImportListModalProps extends EditImportListModalContentProps {
  isOpen: boolean;
}

function EditImportListModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditImportListModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section }));
    dispatch(cancelTestImportList({ section }));
    dispatch(cancelSaveImportList({ section }));

    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleModalClose}>
      <EditImportListModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditImportListModal;
