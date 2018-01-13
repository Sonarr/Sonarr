import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import TagsModalContentConnector from './TagsModalContentConnector';

function TagsModal(props) {
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
      <TagsModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

TagsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default TagsModal;
