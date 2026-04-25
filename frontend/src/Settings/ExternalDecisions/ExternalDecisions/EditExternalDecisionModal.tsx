import React, { useCallback } from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditExternalDecisionModalContent, {
  EditExternalDecisionModalContentProps,
} from './EditExternalDecisionModalContent';

interface EditExternalDecisionModalProps
  extends EditExternalDecisionModalContentProps {
  isOpen: boolean;
}

function EditExternalDecisionModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditExternalDecisionModalProps) {
  const handleModalClose = useCallback(() => {
    onModalClose();
  }, [onModalClose]);

  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={handleModalClose}>
      <EditExternalDecisionModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditExternalDecisionModal;
