import React, { useCallback, useState } from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, scrollDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './SelectReleaseGroupModalContent.module.css';

interface SelectReleaseGroupModalContentProps {
  releaseGroup: string;
  modalTitle: string;
  onReleaseGroupSelect(releaseGroup: string): void;
  onModalClose(): void;
}

function SelectReleaseGroupModalContent(
  props: SelectReleaseGroupModalContentProps
) {
  const { modalTitle, onReleaseGroupSelect, onModalClose } = props;
  const [releaseGroup, setReleaseGroup] = useState(props.releaseGroup);

  const onReleaseGroupChange = useCallback(
    ({ value }: { value: string }) => {
      setReleaseGroup(value);
    },
    [setReleaseGroup]
  );

  const onReleaseGroupSelectWrapper = useCallback(() => {
    onReleaseGroupSelect(releaseGroup);
  }, [releaseGroup, onReleaseGroupSelect]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SetReleaseGroupModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <Form>
          <FormRow>
            <FormLabel>{translate('ReleaseGroup')}</FormLabel>
            <FormInput
              type={inputTypes.TEXT}
              name="releaseGroup"
              value={releaseGroup}
              autoFocus={true}
              onChange={onReleaseGroupChange}
            />
          </FormRow>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.SUCCESS} onPress={onReleaseGroupSelectWrapper}>
          {translate('SetReleaseGroup')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectReleaseGroupModalContent;
