import PropTypes from 'prop-types';
import React from 'react';
import ErrorBoundaryError from 'Components/Error/ErrorBoundaryError';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import styles from './ModalError.css';

function ModalError(props) {
  const {
    onModalClose,
    ...otherProps
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Error
      </ModalHeader>

      <ModalBody>
        <ErrorBoundaryError
          messageClassName={styles.message}
          detailsClassName={styles.details}
          {...otherProps}
          message='There was an error loading this item'
        />
      </ModalBody>

      <ModalFooter>
        <Button
          onPress={onModalClose}
        >
          Close
        </Button>
      </ModalFooter>
    </ModalContent>);
}

ModalError.propTypes = {
  onModalClose: PropTypes.func.isRequired
};

export default ModalError;
