import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditSpecificationModalContent, {
  EditSpecificationModalContentProps,
} from './EditSpecificationModalContent';

const section = 'settings.customFormatSpecifications';

interface EditSpecificationModalProps
  extends EditSpecificationModalContentProps {
  isOpen: boolean;
}

function EditSpecificationModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditSpecificationModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section }));

    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditSpecificationModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditSpecificationModal;
