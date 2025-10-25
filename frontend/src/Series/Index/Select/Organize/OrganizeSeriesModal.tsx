import React from 'react';
import Modal from 'Components/Modal/Modal';
import OrganizeSeriesModalContent, {
  OrganizeSeriesModalContentProps,
} from './OrganizeSeriesModalContent';

interface OrganizeSeriesModalProps extends OrganizeSeriesModalContentProps {
  isOpen: boolean;
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
