import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EpisodeRetentionPreviewModalContent from './EpisodeRetentionPreviewModalContent';

interface EpisodeRetentionPreviewModalProps {
  isOpen: boolean;
  seriesId: number;
  onModalClose: () => void;
  onCleanupStart: () => void;
}

function EpisodeRetentionPreviewModal({
  isOpen,
  seriesId,
  onModalClose,
  onCleanupStart,
}: EpisodeRetentionPreviewModalProps) {
  return (
    <Modal size={sizes.MEDIUM} isOpen={isOpen} onModalClose={onModalClose}>
      <EpisodeRetentionPreviewModalContent
        seriesId={seriesId}
        onModalClose={onModalClose}
        onCleanupStart={onCleanupStart}
      />
    </Modal>
  );
}

export default EpisodeRetentionPreviewModal;
