import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import EpisodeOrderModalContent, {
  EpisodeOrderModalContentProps,
} from './EpisodeOrderModalContent';

interface EpisodeOrderModalProps extends EpisodeOrderModalContentProps {
  isOpen: boolean;
}

function EpisodeOrderModal({ isOpen, ...otherProps }: EpisodeOrderModalProps) {
  return (
    <Modal
      isOpen={isOpen}
      size={sizes.MEDIUM}
      onModalClose={otherProps.onModalClose}
    >
      {isOpen ? <EpisodeOrderModalContent {...otherProps} /> : null}
    </Modal>
  );
}

export default EpisodeOrderModal;
