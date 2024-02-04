import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import AddDownloadClientItem from './AddDownloadClientItem';
import styles from './AddDownloadClientModalContent.css';

class AddDownloadClientModalContent extends Component {

  //
  // Render

  render() {
    const {
      isSchemaFetching,
      isSchemaPopulated,
      schemaError,
      usenetDownloadClients,
      torrentDownloadClients,
      onDownloadClientSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('AddDownloadClient')}
        </ModalHeader>

        <ModalBody>
          {
            isSchemaFetching &&
              <LoadingIndicator />
          }

          {
            !isSchemaFetching && !!schemaError &&
              <Alert kind={kinds.DANGER}>
                {translate('AddDownloadClientError')}
              </Alert>
          }

          {
            isSchemaPopulated && !schemaError &&
              <div>

                <Alert kind={kinds.INFO}>
                  <div>
                    {translate('SupportedDownloadClients')}
                  </div>
                  <div>
                    {translate('SupportedDownloadClientsMoreInfo')}
                  </div>
                </Alert>

                <FieldSet legend={translate('Usenet')}>
                  <div className={styles.downloadClients}>
                    {
                      usenetDownloadClients.map((downloadClient) => {
                        return (
                          <AddDownloadClientItem
                            key={downloadClient.implementation}
                            implementation={downloadClient.implementation}
                            {...downloadClient}
                            onDownloadClientSelect={onDownloadClientSelect}
                          />
                        );
                      })
                    }
                  </div>
                </FieldSet>

                <FieldSet legend={translate('Torrents')}>
                  <div className={styles.downloadClients}>
                    {
                      torrentDownloadClients.map((downloadClient) => {
                        return (
                          <AddDownloadClientItem
                            key={downloadClient.implementation}
                            implementation={downloadClient.implementation}
                            {...downloadClient}
                            onDownloadClientSelect={onDownloadClientSelect}
                          />
                        );
                      })
                    }
                  </div>
                </FieldSet>
              </div>
          }
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
}

AddDownloadClientModalContent.propTypes = {
  isSchemaFetching: PropTypes.bool.isRequired,
  isSchemaPopulated: PropTypes.bool.isRequired,
  schemaError: PropTypes.object,
  usenetDownloadClients: PropTypes.arrayOf(PropTypes.object).isRequired,
  torrentDownloadClients: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDownloadClientSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddDownloadClientModalContent;
