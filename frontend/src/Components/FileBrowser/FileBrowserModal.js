import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import FileBrowserModalContentConnector from './FileBrowserModalContentConnector';
import styles from './FileBrowserModal.css';

class FileBrowserModal extends Component {

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
        className={styles.modal}
        isOpen={isOpen}
        onModalClose={onModalClose}
      >
        <FileBrowserModalContentConnector
          {...otherProps}
          onModalClose={onModalClose}
        />
      </Modal>
    );
  }
}

FileBrowserModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default FileBrowserModal;
