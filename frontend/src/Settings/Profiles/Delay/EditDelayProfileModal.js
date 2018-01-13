import PropTypes from 'prop-types';
import React from 'react';
import { sizes } from 'Helpers/Props';
import Modal from 'Components/Modal/Modal';
import EditDelayProfileModalContentConnector from './EditDelayProfileModalContentConnector';

function EditDelayProfileModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      size={sizes.MEDIUM}
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditDelayProfileModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditDelayProfileModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditDelayProfileModal;
