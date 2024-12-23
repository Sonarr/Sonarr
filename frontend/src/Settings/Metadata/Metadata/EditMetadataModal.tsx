import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditMetadataModalContent, {
  EditMetadataModalContentProps,
} from './EditMetadataModalContent';

interface EditMetadataModalProps
  extends Omit<EditMetadataModalContentProps, 'advancedSettings'> {
  isOpen: boolean;
}

function EditMetadataModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditMetadataModalProps) {
  const dispatch = useDispatch();

  const advancedSettings = useSelector(
    (state: AppState) => state.settings.advancedSettings
  );

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'metadata' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditMetadataModalContent
        {...otherProps}
        advancedSettings={advancedSettings}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditMetadataModal;
