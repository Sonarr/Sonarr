import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import SeriesHistoryModalContentConnector from './SeriesHistoryModalContentConnector';

function SeriesHistoryModal(props) {
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
      <SeriesHistoryModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

SeriesHistoryModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SeriesHistoryModal;
