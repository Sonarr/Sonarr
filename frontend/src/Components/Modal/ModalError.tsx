import React from 'react';
import ErrorBoundaryError, {
  ErrorBoundaryErrorProps,
} from 'Components/Error/ErrorBoundaryError';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import translate from 'Utilities/String/translate';
import styles from './ModalError.css';

interface ModalErrorProps extends ErrorBoundaryErrorProps {
  onModalClose: () => void;
}

function ModalError({ onModalClose, ...otherProps }: ModalErrorProps) {
  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('Error')}</ModalHeader>

      <ModalBody>
        <ErrorBoundaryError
          {...otherProps}
          messageClassName={styles.message}
          detailsClassName={styles.details}
          message={translate('ErrorLoadingItem')}
        />
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default ModalError;
