import React, { useMemo } from 'react';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import { SelectedSchema } from 'Settings/useProviderSchema';
import translate from 'Utilities/String/translate';
import AddDownloadClientItem from './AddDownloadClientItem';
import {
  DownloadClientModel,
  useDownloadClientSchema,
} from './useDownloadClients';
import styles from './AddDownloadClientModalContent.css';

export interface AddDownloadClientModalContentProps {
  onDownloadClientSelect: (selectedSchema: SelectedSchema) => void;
  onModalClose: () => void;
}

function AddDownloadClientModalContent({
  onDownloadClientSelect,
  onModalClose,
}: AddDownloadClientModalContentProps) {
  const { isSchemaLoading, isSchemaFetched, schemaError, schema } =
    useDownloadClientSchema();

  const { usenetDownloadClients, torrentDownloadClients } = useMemo(() => {
    return schema.reduce<{
      usenetDownloadClients: DownloadClientModel[];
      torrentDownloadClients: DownloadClientModel[];
    }>(
      (acc, item) => {
        if (item.protocol === 'usenet') {
          acc.usenetDownloadClients.push(item);
        } else if (item.protocol === 'torrent') {
          acc.torrentDownloadClients.push(item);
        }

        return acc;
      },
      {
        usenetDownloadClients: [],
        torrentDownloadClients: [],
      }
    );
  }, [schema]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AddDownloadClient')}</ModalHeader>

      <ModalBody>
        {isSchemaLoading ? <LoadingIndicator /> : null}

        {!isSchemaLoading && !!schemaError ? (
          <Alert kind={kinds.DANGER}>
            {translate('AddDownloadClientError')}
          </Alert>
        ) : null}

        {isSchemaFetched && !schemaError ? (
          <div className={styles.downloadClients}>
            <div className={styles.groupLegend}>{translate('Usenet')}</div>

            <div>
              {usenetDownloadClients.map((downloadClient) => (
                <AddDownloadClientItem
                  key={downloadClient.implementation}
                  {...downloadClient}
                  implementation={downloadClient.implementation}
                  onDownloadClientSelect={onDownloadClientSelect}
                />
              ))}
            </div>

            <div className={styles.groupLegend}>{translate('Torrents')}</div>

            <div>
              {torrentDownloadClients.map((downloadClient) => (
                <AddDownloadClientItem
                  key={downloadClient.implementation}
                  {...downloadClient}
                  implementation={downloadClient.implementation}
                  onDownloadClientSelect={onDownloadClientSelect}
                />
              ))}
            </div>
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddDownloadClientModalContent;
