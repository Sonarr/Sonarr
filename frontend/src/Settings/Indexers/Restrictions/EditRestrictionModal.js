import PropTypes from 'prop-types';
import React from 'react';
import { sizes } from 'Helpers/Props';
import Modal from 'Components/Modal/Modal';
import EditRestrictionModalContentConnector from './EditRestrictionModalContentConnector';

function EditRestrictionModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      size={sizes.MEDIUM}
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditRestrictionModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditRestrictionModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditRestrictionModal;
