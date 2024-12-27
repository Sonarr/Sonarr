import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import TagDetailsModalContent, {
  TagDetailsModalContentProps,
} from './TagDetailsModalContent';

interface TagDetailsModalProps extends TagDetailsModalContentProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function TagDetailsModal({
  isOpen,
  onModalClose,
  ...otherProps
}: TagDetailsModalProps) {
  return (
    <Modal size={sizes.SMALL} isOpen={isOpen} onModalClose={onModalClose}>
      <TagDetailsModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default TagDetailsModal;
