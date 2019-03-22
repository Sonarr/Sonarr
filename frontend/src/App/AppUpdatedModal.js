import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import AppUpdatedModalContentConnector from './AppUpdatedModalContentConnector';

function AppUpdatedModal(props) {
  const {
    isOpen,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <AppUpdatedModalContentConnector
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

AppUpdatedModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AppUpdatedModal;
