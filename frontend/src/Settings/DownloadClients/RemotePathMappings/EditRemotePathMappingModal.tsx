import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditRemotePathMappingModalContent, {
  EditRemotePathMappingModalContentProps,
} from './EditRemotePathMappingModalContent';

interface EditRemotePathMappingModalProps
  extends EditRemotePathMappingModalContentProps {
  isOpen: boolean;
}

function EditRemotePathMappingModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditRemotePathMappingModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'settings.remotePathMappings' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditRemotePathMappingModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditRemotePathMappingModal;
