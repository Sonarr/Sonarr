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
import { IndexerModel, useIndexerSchema } from '../useIndexers';
import AddIndexerItem from './AddIndexerItem';
import styles from './AddIndexerModalContent.css';

export interface AddIndexerModalContentProps {
  onIndexerSelect: (selectedSchema: SelectedSchema) => void;
  onModalClose: () => void;
}

function AddIndexerModalContent({
  onIndexerSelect,
  onModalClose,
}: AddIndexerModalContentProps) {
  const { isSchemaLoading, isSchemaFetched, schemaError, schema } =
    useIndexerSchema();

  const { usenetIndexers, torrentIndexers } = useMemo(() => {
    return schema.reduce<{
      usenetIndexers: IndexerModel[];
      torrentIndexers: IndexerModel[];
    }>(
      (acc, item) => {
        if (item.protocol === 'usenet') {
          acc.usenetIndexers.push(item);
        } else if (item.protocol === 'torrent') {
          acc.torrentIndexers.push(item);
        }

        return acc;
      },
      {
        usenetIndexers: [],
        torrentIndexers: [],
      }
    );
  }, [schema]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AddIndexer')}</ModalHeader>

      <ModalBody>
        {isSchemaLoading ? <LoadingIndicator /> : null}

        {!isSchemaLoading && !!schemaError ? (
          <Alert kind={kinds.DANGER}>{translate('AddIndexerError')}</Alert>
        ) : null}

        {isSchemaFetched && !schemaError ? (
          <div className={styles.indexers}>
            <div className={styles.group}>
              <h3 className={styles.groupHeading}>{translate('Usenet')}</h3>

              {usenetIndexers.map((indexer) => (
                <AddIndexerItem
                  key={indexer.implementation}
                  {...indexer}
                  implementation={indexer.implementation}
                  onIndexerSelect={onIndexerSelect}
                />
              ))}
            </div>

            <div className={styles.group}>
              <h3 className={styles.groupHeading}>{translate('Torrents')}</h3>

              {torrentIndexers.map((indexer) => (
                <AddIndexerItem
                  key={indexer.implementation}
                  {...indexer}
                  implementation={indexer.implementation}
                  onIndexerSelect={onIndexerSelect}
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

export default AddIndexerModalContent;
