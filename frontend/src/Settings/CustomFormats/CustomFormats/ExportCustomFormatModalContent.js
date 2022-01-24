import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
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
          Export Custom Format
        </ModalHeader>

        <ModalBody>
          <div>
            {
              isFetching &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <div>
                  Unable to load custom formats
                </div>
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
            title="Copy to clipboard"
            kind={kinds.DEFAULT}
          />
          <Button
            onPress={onModalClose}
          >
            Close
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
