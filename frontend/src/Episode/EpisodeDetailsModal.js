import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import EpisodeDetailsModalContentConnector from './EpisodeDetailsModalContentConnector';

class EpisodeDetailsModal extends Component {

  //
  // Render

  render() {
    const {
      isOpen,
      onModalClose,
      ...otherProps
    } = this.props;

    return (
      <Modal
        isOpen={isOpen}
        onModalClose={onModalClose}
      >
        <EpisodeDetailsModalContentConnector
          {...otherProps}
          onModalClose={onModalClose}
        />
      </Modal>
    );
  }
}

EpisodeDetailsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EpisodeDetailsModal;
