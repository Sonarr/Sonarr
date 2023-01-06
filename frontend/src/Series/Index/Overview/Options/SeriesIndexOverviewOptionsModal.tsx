import React from 'react';
import Modal from 'Components/Modal/Modal';
import SeriesIndexOverviewOptionsModalContent from './SeriesIndexOverviewOptionsModalContent';

interface SeriesIndexOverviewOptionsModalProps {
  isOpen: boolean;
  onModalClose(...args: unknown[]): void;
}

function SeriesIndexOverviewOptionsModal({
  isOpen,
  onModalClose,
  ...otherProps
}: SeriesIndexOverviewOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SeriesIndexOverviewOptionsModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SeriesIndexOverviewOptionsModal;
