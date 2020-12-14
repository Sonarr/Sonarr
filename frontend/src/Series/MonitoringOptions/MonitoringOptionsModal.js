import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import MonitoringOptionsModalContentConnector from './MonitoringOptionsModalContentConnector';

function MonitoringOptionsModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <MonitoringOptionsModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

MonitoringOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MonitoringOptionsModal;
