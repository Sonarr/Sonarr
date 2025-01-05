import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditCustomFormatModalContent, {
  EditCustomFormatModalContentProps,
} from './EditCustomFormatModalContent';

interface EditCustomFormatModalProps extends EditCustomFormatModalContentProps {
  isOpen: boolean;
}

function EditCustomFormatModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditCustomFormatModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'settings.customFormats' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} size={sizes.LARGE} onModalClose={handleModalClose}>
      <EditCustomFormatModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditCustomFormatModal;
