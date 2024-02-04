import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ExportCustomFormatModalContent.css';

class ExportCustomFormatModalContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      json,
      specificationsPopulated,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>

        <ModalHeader>
          {translate('ExportCustomFormat')}
        </ModalHeader>

        <ModalBody>
          <div>
            {
              isFetching &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <Alert kind={kinds.DANGER}>
                  {translate('CustomFormatsLoadError')}
                </Alert>
            }

            {
              !isFetching && !error && specificationsPopulated &&
                <div>
                  <pre>
                    {json}
                  </pre>
                </div>
            }
          </div>
        </ModalBody>
        <ModalFooter>
          <ClipboardButton
            className={styles.button}
            value={json}
            title={translate('CopyToClipboard')}
            kind={kinds.DEFAULT}
          />
          <Button
            onPress={onModalClose}
          >
            {translate('Close')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

ExportCustomFormatModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  json: PropTypes.string.isRequired,
  specificationsPopulated: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ExportCustomFormatModalContent;
