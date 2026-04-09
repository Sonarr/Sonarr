import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EditAutoTaggingModalContent, {
  EditAutoTaggingModalContentProps,
} from './EditAutoTaggingModalContent';

interface EditAutoTaggingModalProps extends EditAutoTaggingModalContentProps {
  isOpen: boolean;
  onModalClose: () => void;
}

export default function EditAutoTaggingModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditAutoTaggingModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.LARGE} onModalClose={onModalClose}>
      <EditAutoTaggingModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}
