import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditSpecificationModalContent, {
  EditSpecificationModalContentProps,
} from './EditSpecificationModalContent';

interface EditSpecificationModalProps
  extends EditSpecificationModalContentProps {
  isOpen: boolean;
}

function EditSpecificationModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditSpecificationModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EditSpecificationModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditSpecificationModal;
