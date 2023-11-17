import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ConnectionLostModal.css';

function ConnectionLostModal(props) {
  const {
    isOpen,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('ConnectionLost')}
        </ModalHeader>

        <ModalBody>
          <div>
            {translate('ConnectionLostToBackend')}
          </div>

          <div className={styles.automatic}>
            {translate('ConnectionLostReconnect')}
          </div>
        </ModalBody>
        <ModalFooter>
          <Button
            kind={kinds.PRIMARY}
            onPress={onModalClose}
          >
            {translate('Reload')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

ConnectionLostModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ConnectionLostModal;
