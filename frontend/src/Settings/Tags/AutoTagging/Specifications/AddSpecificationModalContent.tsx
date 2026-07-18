import React, { useCallback } from 'react';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import {
  AutoTaggingSpecification,
  useAutoTaggingSchema,
} from '../useAutoTaggings';
import AddSpecificationItem from './AddSpecificationItem';
import styles from './AddSpecificationModalContent.css';

type SchemaItem = AutoTaggingSpecification & {
  infoLink?: string;
  presets?: AutoTaggingSpecification[];
};

export interface AddSpecificationModalContentProps {
  onModalClose: (selectedSpec?: AutoTaggingSpecification) => void;
}

function AddSpecificationModalContent({
  onModalClose,
}: AddSpecificationModalContentProps) {
  const schemaResult = useAutoTaggingSchema();
  const schema = schemaResult.schema as SchemaItem[];
  const { isSchemaLoading, schemaError } = schemaResult;

  const onSpecificationSelect = useCallback(
    ({
      implementation,
      presetName,
    }: {
      implementation: string;
      presetName?: string;
    }) => {
      const selected = schema.find((s) => s.implementation === implementation);

      if (!selected) {
        return;
      }

      if (presetName) {
        const preset = selected.presets?.find((p) => p.name === presetName);

        if (preset) {
          onModalClose({ ...preset });
          return;
        }
      }

      const { presets: _unused, ...rest } = selected;
      onModalClose(rest as AutoTaggingSpecification);
    },
    [schema, onModalClose]
  );

  const handleModalClose = useCallback(() => {
    onModalClose();
  }, [onModalClose]);

  return (
    <ModalContent onModalClose={handleModalClose}>
      <ModalHeader>{translate('AddCondition')}</ModalHeader>

      <ModalBody>
        {isSchemaLoading ? <LoadingIndicator /> : null}

        {!isSchemaLoading && schemaError ? (
          <Alert kind={kinds.DANGER}>{translate('AddConditionError')}</Alert>
        ) : null}

        {!isSchemaLoading && !schemaError && schema.length ? (
          <div className={styles.specifications}>
            {schema.map((specification) => (
              <AddSpecificationItem
                key={specification.implementation}
                implementation={specification.implementation}
                implementationName={specification.implementationName}
                infoLink={specification.infoLink}
                presets={specification.presets}
                onSpecificationSelect={onSpecificationSelect}
              />
            ))}
          </div>
        ) : null}
      </ModalBody>

      <ModalFooter>
        <Button onPress={handleModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default AddSpecificationModalContent;
