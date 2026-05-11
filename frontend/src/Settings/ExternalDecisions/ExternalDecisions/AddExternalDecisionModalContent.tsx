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
import { useExternalDecisionSchema } from '../useExternalDecisions';
import AddExternalDecisionItem from './AddExternalDecisionItem';
import styles from './AddExternalDecisionModalContent.css';

export interface AddExternalDecisionModalContentProps {
  onExternalDecisionSelect: (selectedSchema: SelectedSchema) => void;
  onModalClose: () => void;
}

function AddExternalDecisionModalContent({
  onExternalDecisionSelect,
  onModalClose,
}: AddExternalDecisionModalContentProps) {
  const { isSchemaFetching, isSchemaFetched, schemaError, schema } =
    useExternalDecisionSchema();

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AddExternalDecision')}</ModalHeader>

      <ModalBody>
        {isSchemaFetching && !isSchemaFetched ? <LoadingIndicator /> : null}

        {!isSchemaFetching && !!schemaError ? (
          <Alert kind={kinds.DANGER}>
            {translate('AddExternalDecisionError')}
          </Alert>
        ) : null}

        {isSchemaFetched && !schemaError ? (
          <div className={styles.externalDecisions}>
            {schema.map((schema) => {
              return (
                <AddExternalDecisionItem
                  key={schema.implementation}
                  {...schema}
                  implementation={schema.implementation}
                  onExternalDecisionSelect={onExternalDecisionSelect}
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

export default AddExternalDecisionModalContent;
