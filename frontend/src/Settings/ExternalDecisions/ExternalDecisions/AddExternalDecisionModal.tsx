import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddExternalDecisionModalContent, {
  AddExternalDecisionModalContentProps,
} from './AddExternalDecisionModalContent';

interface AddExternalDecisionModalProps
  extends AddExternalDecisionModalContentProps {
  isOpen: boolean;
}

function AddExternalDecisionModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddExternalDecisionModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddExternalDecisionModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default AddExternalDecisionModal;
