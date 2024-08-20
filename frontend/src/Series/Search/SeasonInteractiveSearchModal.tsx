import React, { useCallback, useEffect } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import SeasonInteractiveSearchModalContent from './SeasonInteractiveSearchModalContent';

interface SeasonInteractiveSearchModalProps {
  isOpen: boolean;
  seriesId: number;
  seasonNumber: number;
  onModalClose(): void;
}

function SeasonInteractiveSearchModal(
  props: SeasonInteractiveSearchModalProps
) {
  const { isOpen, seriesId, seasonNumber, onModalClose } = props;

  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(cancelFetchReleases());
    dispatch(clearReleases());

    onModalClose();
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
        seriesId={seriesId}
        seasonNumber={seasonNumber}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default SeasonInteractiveSearchModal;
