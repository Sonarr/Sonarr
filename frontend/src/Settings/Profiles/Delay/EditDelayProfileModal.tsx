import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditDelayProfileModalContent, {
  EditDelayProfileModalContentProps,
} from './EditDelayProfileModalContent';

interface EditDelayProfileModalProps extends EditDelayProfileModalContentProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function EditDelayProfileModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditDelayProfileModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'settings.delayProfiles' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditDelayProfileModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditDelayProfileModal;
