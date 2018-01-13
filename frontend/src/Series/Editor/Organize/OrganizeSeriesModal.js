import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import OrganizeSeriesModalContentConnector from './OrganizeSeriesModalContentConnector';

function OrganizeSeriesModal(props) {
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
      <OrganizeSeriesModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

OrganizeSeriesModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default OrganizeSeriesModal;
