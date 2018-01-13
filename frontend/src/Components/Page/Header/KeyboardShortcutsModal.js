import PropTypes from 'prop-types';
import React from 'react';
import { sizes } from 'Helpers/Props';
import Modal from 'Components/Modal/Modal';
import KeyboardShortcutsModalContentConnector from './KeyboardShortcutsModalContentConnector';

function KeyboardShortcutsModal(props) {
  const {
    isOpen,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.SMALL}
      onModalClose={onModalClose}
    >
      <KeyboardShortcutsModalContentConnector
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

KeyboardShortcutsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default KeyboardShortcutsModal;
