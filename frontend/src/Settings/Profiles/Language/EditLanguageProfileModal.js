import PropTypes from 'prop-types';
import React from 'react';
import { sizes } from 'Helpers/Props';
import Modal from 'Components/Modal/Modal';
import EditLanguageProfileModalContentConnector from './EditLanguageProfileModalContentConnector';

function EditLanguageProfileModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      size={sizes.MEDIUM}
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditLanguageProfileModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditLanguageProfileModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditLanguageProfileModal;
