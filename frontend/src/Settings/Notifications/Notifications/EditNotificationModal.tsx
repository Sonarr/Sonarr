import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditNotificationModalContent, {
  EditNotificationModalContentProps,
} from './EditNotificationModalContent';

const section = 'settings.notifications';

interface EditNotificationModalProps extends EditNotificationModalContentProps {
  isOpen: boolean;
}

function EditNotificationModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditNotificationModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditNotificationModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditNotificationModal;
