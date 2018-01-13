import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import RestoreBackupModalContentConnector from './RestoreBackupModalContentConnector';

function RestoreBackupModal(props) {
  const {
    isOpen,
    onModalClose,
    ...otherProps
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <RestoreBackupModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

RestoreBackupModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RestoreBackupModal;
