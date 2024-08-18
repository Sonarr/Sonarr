import React, { useCallback, useState } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ManageCustomFormatsEditModalContent.css';

interface SavePayload {
  includeCustomFormatWhenRenaming?: boolean;
}

interface ManageCustomFormatsEditModalContentProps {
  customFormatIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const enableOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
  },
  {
    key: 'enabled',
    get value() {
      return translate('Enabled');
    },
  },
  {
    key: 'disabled',
    get value() {
      return translate('Disabled');
    },
  },
];

function ManageCustomFormatsEditModalContent(
  props: ManageCustomFormatsEditModalContentProps
) {
  const { customFormatIds, onSavePress, onModalClose } = props;

  const [includeCustomFormatWhenRenaming, setIncludeCustomFormatWhenRenaming] =
    useState(NO_CHANGE);

  const save = useCallback(() => {
    let hasChanges = false;
    const payload: SavePayload = {};

    if (includeCustomFormatWhenRenaming !== NO_CHANGE) {
      hasChanges = true;
      payload.includeCustomFormatWhenRenaming =
        includeCustomFormatWhenRenaming === 'enabled';
    }

    if (hasChanges) {
      onSavePress(payload);
    }

    onModalClose();
  }, [includeCustomFormatWhenRenaming, onSavePress, onModalClose]);

  const onInputChange = useCallback(
    ({ name, value }: { name: string; value: string }) => {
      switch (name) {
        case 'includeCustomFormatWhenRenaming':
          setIncludeCustomFormatWhenRenaming(value);
          break;
        default:
          console.warn(
            `EditCustomFormatsModalContent Unknown Input: '${name}'`
          );
      }
    },
    []
  );

  const selectedCount = customFormatIds.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedCustomFormats')}</ModalHeader>

      <ModalBody>
        <FormGroup>
          <FormLabel>{translate('IncludeCustomFormatWhenRenaming')}</FormLabel>

          <FormInputGroup
            type={inputTypes.SELECT}
            name="includeCustomFormatWhenRenaming"
            value={includeCustomFormatWhenRenaming}
            values={enableOptions}
            onChange={onInputChange}
          />
        </FormGroup>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('CountCustomFormatsSelected', {
            count: selectedCount,
          })}
        </div>

        <div>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button onPress={save}>{translate('ApplyChanges')}</Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default ManageCustomFormatsEditModalContent;
