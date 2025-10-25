import React from 'react';
import Modal from 'Components/Modal/Modal';
import ChangeMonitoringModalContent, {
  ChangeMonitoringModalContentProps,
} from './ChangeMonitoringModalContent';

interface ChangeMonitoringModalProps extends ChangeMonitoringModalContentProps {
  isOpen: boolean;
}

function ChangeMonitoringModal({
  isOpen,
  onSavePress,
  onModalClose,
}: ChangeMonitoringModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ChangeMonitoringModalContent
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default ChangeMonitoringModal;
