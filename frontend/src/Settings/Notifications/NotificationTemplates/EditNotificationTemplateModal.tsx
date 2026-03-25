import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditNotificationTemplateModalContent from './EditNotificationTemplateModalContent';

interface EditNotificationTemplateModalProps {
  id?: number;
  isOpen: boolean;
  onModalClose: () => void;
  onDeleteNotificationTemplatePress?: () => void;
}

function EditNotificationTemplateModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditNotificationTemplateModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(
      clearPendingChanges({
        section: 'settings.notificationTemplates',
      })
    );
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditNotificationTemplateModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditNotificationTemplateModal;
