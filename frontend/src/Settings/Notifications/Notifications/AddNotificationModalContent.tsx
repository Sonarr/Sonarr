import React from 'react';
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
import { useConnectionSchema } from '../useConnections';
import AddNotificationItem from './AddNotificationItem';
import styles from './AddNotificationModalContent.css';

export interface AddNotificationModalContentProps {
  onNotificationSelect: (selectedScehema: SelectedSchema) => void;
  onModalClose: () => void;
}

function AddNotificationModalContent({
  onNotificationSelect,
  onModalClose,
}: AddNotificationModalContentProps) {
  const { isSchemaFetching, isSchemaFetched, schemaError, schema } =
    useConnectionSchema();

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AddConnection')}</ModalHeader>

      <ModalBody>
        {isSchemaFetching && !isSchemaFetched ? <LoadingIndicator /> : null}

        {!isSchemaFetching && !!schemaError ? (
          <Alert kind={kinds.DANGER}>{translate('AddConnectionError')}</Alert>
        ) : null}

        {isSchemaFetched && !schemaError ? (
          <div className={styles.notifications}>
            {schema.map((notification) => {
              return (
                <AddNotificationItem
                  key={notification.implementation}
                  {...notification}
                  implementation={notification.implementation}
                  onNotificationSelect={onNotificationSelect}
                />
              );
            })}
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddNotificationModalContent;
