import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditSeriesModalContent, {
  EditSeriesModalContentProps,
} from './EditSeriesModalContent';

interface EditSeriesModalProps extends EditSeriesModalContentProps {
  isOpen: boolean;
}

function EditSeriesModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditSeriesModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'series' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleModalClose}>
      <EditSeriesModalContent {...otherProps} onModalClose={handleModalClose} />
    </Modal>
  );
}

export default EditSeriesModal;
