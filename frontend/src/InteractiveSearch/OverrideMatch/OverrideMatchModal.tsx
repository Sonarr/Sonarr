import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import OverrideMatchModalContent, {
  OverrideMatchModalContentProps,
} from './OverrideMatchModalContent';

interface OverrideMatchModalProps extends OverrideMatchModalContentProps {
  isOpen: boolean;
}

function OverrideMatchModal({
  isOpen,
  onModalClose,
  ...otherProps
}: OverrideMatchModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.LARGE} onModalClose={onModalClose}>
      <OverrideMatchModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default OverrideMatchModal;
