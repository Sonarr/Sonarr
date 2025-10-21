import React from 'react';
import Modal from 'Components/Modal/Modal';
import ManualSearchModalContent from './ManualSearchModalContent';

interface ManualSearchModalProps {
  isOpen: boolean;
  episodeId: number;
  episodeTitle: string;
  onModalClose: () => void;
}

function ManualSearchModal({
  isOpen,
  episodeId,
  episodeTitle,
  onModalClose,
}: ManualSearchModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ManualSearchModalContent
        episodeId={episodeId}
        episodeTitle={episodeTitle}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default ManualSearchModal;
