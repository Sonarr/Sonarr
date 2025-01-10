import React from 'react';
import Modal from 'Components/Modal/Modal';
import MonitoringOptionsModalContent, {
  MonitoringOptionsModalContentProps,
} from './MonitoringOptionsModalContent';

interface MonitoringOptionsModalProps
  extends MonitoringOptionsModalContentProps {
  isOpen: boolean;
}

function MonitoringOptionsModal({
  isOpen,
  onModalClose,
  ...otherProps
}: MonitoringOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <MonitoringOptionsModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default MonitoringOptionsModal;
