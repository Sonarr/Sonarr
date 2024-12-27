import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditSpecificationModalContent, {
  EditSpecificationModalContentProps,
} from './EditSpecificationModalContent';

interface EditSpecificationModalProps
  extends EditSpecificationModalContentProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function EditSpecificationModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditSpecificationModalProps) {
  const dispatch = useDispatch();

  const onWrappedModalClose = useCallback(() => {
    dispatch(
      clearPendingChanges({ section: 'settings.autoTaggingSpecifications' })
    );
    onModalClose();
  }, [onModalClose, dispatch]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditSpecificationModalContent
        {...otherProps}
        onModalClose={onWrappedModalClose}
      />
    </Modal>
  );
}

export default EditSpecificationModal;
