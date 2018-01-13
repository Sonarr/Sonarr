import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import CalendarOptionsModalContentConnector from './CalendarOptionsModalContentConnector';

function CalendarOptionsModal(props) {
  const {
    isOpen,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <CalendarOptionsModalContentConnector
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

CalendarOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default CalendarOptionsModal;
