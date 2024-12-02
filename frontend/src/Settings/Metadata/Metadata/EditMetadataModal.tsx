import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditMetadataModalContent, {
  EditMetadataModalContentProps,
} from './EditMetadataModalContent';

interface EditMetadataModalProps extends EditMetadataModalContentProps {
  isOpen: boolean;
}

function EditMetadataModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditMetadataModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'metadata' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditMetadataModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditMetadataModal;
