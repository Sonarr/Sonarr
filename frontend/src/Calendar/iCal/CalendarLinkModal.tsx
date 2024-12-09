import React from 'react';
import Modal from 'Components/Modal/Modal';
import CalendarLinkModalContent from './CalendarLinkModalContent';

interface CalendarLinkModalProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function CalendarLinkModal(props: CalendarLinkModalProps) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <CalendarLinkModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default CalendarLinkModal;
