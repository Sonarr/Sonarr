import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import SelectEpisodeModalContentConnector from './SelectEpisodeModalContentConnector';

class SelectEpisodeModal extends Component {

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
        <SelectEpisodeModalContentConnector
          {...otherProps}
          onModalClose={onModalClose}
        />
      </Modal>
    );
  }
}

SelectEpisodeModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectEpisodeModal;
