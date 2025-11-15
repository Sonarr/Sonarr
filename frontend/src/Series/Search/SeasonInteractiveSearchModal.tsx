import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import SeasonInteractiveSearchModalContent, {
  SeasonInteractiveSearchModalContentProps,
} from './SeasonInteractiveSearchModalContent';

interface SeasonInteractiveSearchModalProps
  extends SeasonInteractiveSearchModalContentProps {
  isOpen: boolean;
}

function SeasonInteractiveSearchModal(
  props: SeasonInteractiveSearchModalProps
) {
  const { isOpen, episodeCount, seriesId, seasonNumber, onModalClose } = props;

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_EXTRA_LARGE}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <SeasonInteractiveSearchModalContent
        episodeCount={episodeCount}
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SeasonInteractiveSearchModal;
