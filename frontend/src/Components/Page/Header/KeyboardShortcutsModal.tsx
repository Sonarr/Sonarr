import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import KeyboardShortcutsModalContent from './KeyboardShortcutsModalContent';

interface KeyboardShortcutsModalProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function KeyboardShortcutsModal(props: KeyboardShortcutsModalProps) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} size={sizes.SMALL} onModalClose={onModalClose}>
      <KeyboardShortcutsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default KeyboardShortcutsModal;
