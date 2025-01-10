import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import SeriesHistoryModalContent, {
  SeriesHistoryModalContentProps,
} from './SeriesHistoryModalContent';

interface SeriesHistoryModalProps extends SeriesHistoryModalContentProps {
  isOpen: boolean;
}

function SeriesHistoryModal({
  isOpen,
  onModalClose,
  ...otherProps
}: SeriesHistoryModalProps) {
  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_EXTRA_LARGE}
      onModalClose={onModalClose}
    >
      <SeriesHistoryModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default SeriesHistoryModal;
