import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import { scrollDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './LogsTableDetailsModal.css';

function LogsTableDetailsModal(props) {
  const {
    isOpen,
    message,
    exception,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('Details')}
        </ModalHeader>

        <ModalBody>
          <div>
            {translate('Message')}
          </div>

          <Scroller
            className={styles.detailsText}
            scrollDirection={scrollDirections.HORIZONTAL}
          >
            {message}
          </Scroller>

          {
            !!exception &&
              <div>
                <div>
                  {translate('Exception')}
                </div>
                <Scroller
                  className={styles.detailsText}
                  scrollDirection={scrollDirections.HORIZONTAL}
                >
                  {exception}
                </Scroller>
              </div>
          }
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

LogsTableDetailsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  message: PropTypes.string.isRequired,
  exception: PropTypes.string,
  onModalClose: PropTypes.func.isRequired
};

export default LogsTableDetailsModal;
