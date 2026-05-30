import React from 'react';
import Modal from 'Components/Modal/Modal';
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
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditSeriesModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default EditSeriesModal;
