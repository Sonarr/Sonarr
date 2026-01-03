import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import Metadata from 'typings/Metadata';
import translate from 'Utilities/String/translate';
import { useManageMetadata } from '../useMetadata';
import styles from './EditMetadataModalContent.css';

export interface EditMetadataModalContentProps {
  id: number;
  advancedSettings: boolean;
  onModalClose: () => void;
}

function EditMetadataModalContent({
  id,
  advancedSettings,
  onModalClose,
}: EditMetadataModalContentProps) {
  const {
    item,
    updateValue,
    updateFieldValue,
    saveProvider,
    isSaving,
    saveError,
    ...otherSettings
  } = useManageMetadata(id);

  const wasSaving = usePrevious(isSaving);

  const { name, enable, fields, message } = item;

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      const key = name as keyof Metadata;

      updateValue(key, value as Metadata[typeof key]);
    },
    [updateValue]
  );

  const handleFieldChange = useCallback(
    ({ name, value }: InputChanged) => {
      updateFieldValue?.({ [name]: value });
    },
    [updateFieldValue]
  );

  const handleSavePress = useCallback(() => {
    saveProvider();
  }, [saveProvider]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('EditMetadata', { metadataType: name.value })}
      </ModalHeader>

      <ModalBody>
        <Form {...otherSettings}>
          {message ? (
            <Alert className={styles.message} kind={message.value.type}>
              {message.value.message}
            </Alert>
          ) : null}

          <FormGroup>
            <FormLabel>{translate('Enable')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enable"
              helpText={translate('EnableMetadataHelpText')}
              {...enable}
              onChange={handleInputChange}
            />
          </FormGroup>

          {fields.map((field) => {
            return (
              <ProviderFieldFormGroup
                key={field.name}
                advancedSettings={advancedSettings}
                provider="metadata"
                {...field}
                isDisabled={!enable.value}
                onChange={handleFieldChange}
              />
            );
          })}
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditMetadataModalContent;
