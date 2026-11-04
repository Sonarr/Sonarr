import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EpisodeRetentionModalContent from './EpisodeRetentionModalContent';

interface EpisodeRetentionModalProps {
  isOpen: boolean;
  seriesId: number;
  onModalClose: () => void;
}

function EpisodeRetentionModal({
  isOpen,
  seriesId,
  onModalClose,
}: EpisodeRetentionModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EpisodeRetentionModalContent
        seriesId={seriesId}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EpisodeRetentionModal;
