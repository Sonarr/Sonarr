import React, { useCallback, useEffect } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
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

  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    onModalClose();

    dispatch(cancelFetchReleases());
    dispatch(clearReleases());
  }, [dispatch, onModalClose]);

  useEffect(() => {
    return () => {
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
    };
  }, [dispatch]);

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_EXTRA_LARGE}
      closeOnBackgroundClick={false}
      onModalClose={handleModalClose}
    >
      <SeasonInteractiveSearchModalContent
        episodeCount={episodeCount}
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default SeasonInteractiveSearchModal;
