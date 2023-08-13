import PropTypes from 'prop-types';
import React from 'react';
import ErrorBoundaryError from 'Components/Error/ErrorBoundaryError';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import translate from 'Utilities/String/translate';
import styles from './ModalError.css';

function ModalError(props) {
  const {
    onModalClose,
    ...otherProps
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('Error')}
      </ModalHeader>

      <ModalBody>
        <ErrorBoundaryError
          messageClassName={styles.message}
          detailsClassName={styles.details}
          {...otherProps}
          message={translate('ErrorLoadingItem')}
        />
      </ModalBody>

      <ModalFooter>
        <Button
          onPress={onModalClose}
        >
          {translate('Close')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

ModalError.propTypes = {
  onModalClose: PropTypes.func.isRequired
};

export default ModalError;
