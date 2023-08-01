import React, { useCallback, useState } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, scrollDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './SelectIndexerFlagsModalContent.css';

interface SelectIndexerFlagsModalContentProps {
  indexerFlags: number;
  modalTitle: string;
  onIndexerFlagsSelect(indexerFlags: number): void;
  onModalClose(): void;
}

function SelectIndexerFlagsModalContent(
  props: SelectIndexerFlagsModalContentProps
) {
  const { modalTitle, onIndexerFlagsSelect, onModalClose } = props;
  const [indexerFlags, setIndexerFlags] = useState(props.indexerFlags);

  const onIndexerFlagsChange = useCallback(
    ({ value }: { value: number }) => {
      setIndexerFlags(value);
    },
    [setIndexerFlags]
  );

  const onIndexerFlagsSelectWrapper = useCallback(() => {
    onIndexerFlagsSelect(indexerFlags);
  }, [indexerFlags, onIndexerFlagsSelect]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SetIndexerFlagsModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <Form>
          <FormGroup>
            <FormLabel>{translate('IndexerFlags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.INDEXER_FLAGS_SELECT}
              name="indexerFlags"
              indexerFlags={indexerFlags}
              autoFocus={true}
              onChange={onIndexerFlagsChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.SUCCESS} onPress={onIndexerFlagsSelectWrapper}>
          {translate('SetIndexerFlags')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectIndexerFlagsModalContent;
