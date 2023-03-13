import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import SeasonInteractiveSearchModalContent from './SeasonInteractiveSearchModalContent';

function SeasonInteractiveSearchModal(props) {
  const {
    isOpen,
    seriesId,
    seasonNumber,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_LARGE}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <SeasonInteractiveSearchModalContent
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

SeasonInteractiveSearchModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  seriesId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SeasonInteractiveSearchModal;
