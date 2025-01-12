import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddNewSeriesModalContent, {
  AddNewSeriesModalContentProps,
} from './AddNewSeriesModalContent';

interface AddNewSeriesModalProps extends AddNewSeriesModalContentProps {
  isOpen: boolean;
}

function AddNewSeriesModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddNewSeriesModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <AddNewSeriesModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default AddNewSeriesModal;
