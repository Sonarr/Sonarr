import React from 'react';
import Modal from 'Components/Modal/Modal';
import ChangeMonitoringModalContent from './ChangeMonitoringModalContent';

interface ChangeMonitoringModalProps {
  isOpen: boolean;
  seriesIds: number[];
  onSavePress(monitor: string): void;
  onModalClose(): void;
}

function ChangeMonitoringModal(props: ChangeMonitoringModalProps) {
  const { isOpen, seriesIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ChangeMonitoringModalContent
        seriesIds={seriesIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default ChangeMonitoringModal;
