import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import SeriesIndexOverviewOptionsModalContentConnector from './SeriesIndexOverviewOptionsModalContentConnector';

function SeriesIndexOverviewOptionsModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <SeriesIndexOverviewOptionsModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

SeriesIndexOverviewOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SeriesIndexOverviewOptionsModal;
