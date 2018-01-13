import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import AddDownloadClientItem from './AddDownloadClientItem';
import styles from './AddDownloadClientModalContent.css';

class AddDownloadClientModalContent extends Component {

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      isPopulated,
      usenetDownloadClients,
      torrentDownloadClients,
      onDownloadClientSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Add DownloadClient
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to add a new downloadClient, please try again.</div>
          }

          {
            isPopulated && !error &&
              <div>

                <Alert kind={kinds.INFO}>
                  <div>Sonarr supports any downloadClient that uses the Newznab standard, as well as other downloadClients listed below.</div>
                  <div>For more information on the individual downloadClients, clink on the info buttons.</div>
                </Alert>

                <FieldSet legend="Usenet">
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

                <FieldSet legend="Torrents">
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
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddDownloadClientModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isPopulated: PropTypes.bool.isRequired,
  usenetDownloadClients: PropTypes.arrayOf(PropTypes.object).isRequired,
  torrentDownloadClients: PropTypes.arrayOf(PropTypes.object).isRequired,
  onDownloadClientSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddDownloadClientModalContent;
