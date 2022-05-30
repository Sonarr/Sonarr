import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import AuthenticationRequiredModalContentConnector from './AuthenticationRequiredModalContentConnector';

function onModalClose() {
  // No-op
}

function AuthenticationRequiredModal(props) {
  const {
    isOpen
  } = props;

  return (
    <Modal
      size={sizes.MEDIUM}
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <AuthenticationRequiredModalContentConnector
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

AuthenticationRequiredModal.propTypes = {
  isOpen: PropTypes.bool.isRequired
};

export default AuthenticationRequiredModal;
