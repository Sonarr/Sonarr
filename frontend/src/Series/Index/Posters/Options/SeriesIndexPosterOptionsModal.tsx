import React from 'react';
import Modal from 'Components/Modal/Modal';
import SeriesIndexPosterOptionsModalContent from './SeriesIndexPosterOptionsModalContent';

interface SeriesIndexPosterOptionsModalProps {
  isOpen: boolean;
  onModalClose(...args: unknown[]): unknown;
}

function SeriesIndexPosterOptionsModal({
  isOpen,
  onModalClose,
}: SeriesIndexPosterOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SeriesIndexPosterOptionsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default SeriesIndexPosterOptionsModal;
