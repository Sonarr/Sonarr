import React from 'react';
import Modal from 'Components/Modal/Modal';
import TagsModalContent, { TagsModalContentProps } from './TagsModalContent';

interface TagsModalProps extends TagsModalContentProps {
  isOpen: boolean;
}

function TagsModal(props: TagsModalProps) {
  const { isOpen, onModalClose, ...otherProps } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <TagsModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default TagsModal;
