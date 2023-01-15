import React from 'react';
import Modal from 'Components/Modal/Modal';
import OrganizeSeriesModalContent from './OrganizeSeriesModalContent';

interface OrganizeSeriesModalProps {
  isOpen: boolean;
  seriesIds: number[];
  onModalClose: () => void;
}

function OrganizeSeriesModal(props: OrganizeSeriesModalProps) {
  const { isOpen, onModalClose, ...otherProps } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <OrganizeSeriesModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default OrganizeSeriesModal;
