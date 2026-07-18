import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalSection from 'Components/ModalSection';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { SelectedSchema } from 'Settings/useProviderSchema';
import { EnhancedSelectInputChanged, InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useManageDownloadClient } from './useDownloadClients';
import styles from './EditDownloadClientModalContent.css';

export interface EditDownloadClientModalContentProps {
  id?: number;
  cloneId?: number;
  selectedSchema?: SelectedSchema;
  onModalClose: () => void;
  onDeleteDownloadClientPress?: () => void;
}

function EditDownloadClientModalContent({
  id,
  cloneId,
  selectedSchema,
  onModalClose,
  onDeleteDownloadClientPress,
}: EditDownloadClientModalContentProps) {
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    item,
    updateFieldValue,
    updateValue,
    saveProvider,
    isSaving,
    saveError,
    testProvider,
    isTesting,
    validationErrors,
    validationWarnings,
  } = useManageDownloadClient(id, cloneId, selectedSchema);

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName = '',
    name,
    enable,
    priority,
    removeCompletedDownloads,
    removeFailedDownloads,
    fields,
    tags,
    message,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - InputChanged is not typed correctly
      updateValue(change.name, change.value);
    },
    [updateValue]
  );

  const handleFieldChange = useCallback(
    ({
      name,
      value,
      additionalProperties,
    }: EnhancedSelectInputChanged<unknown>) => {
      updateFieldValue({ [name]: value, ...additionalProperties });
    },
    [updateFieldValue]
  );

  const handleSavePress = useCallback(() => {
    saveProvider();
  }, [saveProvider]);

  const handleTestPress = useCallback(() => {
    testProvider();
  }, [testProvider]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
          ? translate('EditDownloadClientImplementation', {
              implementationName,
            })
          : translate('AddDownloadClientImplementation', {
              implementationName,
            })}
      </ModalHeader>

      <ModalBody>
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          {message ? (
            <Alert className={styles.message} kind={message.value.type}>
              {message.value.message}
            </Alert>
          ) : null}

          <FormRow>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInput
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Enable')}</FormLabel>

            <FormInput
              type={inputTypes.CHECK}
              name="enable"
              {...enable}
              onChange={handleInputChange}
            />
          </FormRow>

          {fields?.map((field) => {
            return (
              <ProviderFieldFormGroup
                key={field.name}
                advancedSettings={showAdvancedSettings}
                provider="downloadClient"
                providerData={item}
                layout="row"
                {...field}
                onChange={handleFieldChange}
              />
            );
          })}

          <FormRow advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('ClientPriority')}</FormLabel>

            <FormInputHelpText
              text={translate('DownloadClientPriorityHelpText')}
            />

            <FormInput
              type={inputTypes.NUMBER}
              name="priority"
              min={1}
              max={50}
              {...priority}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputHelpText
              text={translate('DownloadClientSeriesTagHelpText')}
            />

            <FormInput
              type={inputTypes.TAG}
              name="tags"
              {...tags}
              onChange={handleInputChange}
            />
          </FormRow>

          <ModalSection title={translate('CompletedDownloadHandling')}>
            <FormRow>
              <FormLabel>{translate('RemoveCompleted')}</FormLabel>

              <FormInputHelpText
                text={translate('RemoveCompletedDownloadsHelpText')}
              />

              <FormInput
                type={inputTypes.CHECK}
                name="removeCompletedDownloads"
                {...removeCompletedDownloads}
                onChange={handleInputChange}
              />
            </FormRow>

            <FormRow>
              <FormLabel>{translate('RemoveFailed')}</FormLabel>

              <FormInputHelpText
                text={translate('RemoveFailedDownloadsHelpText')}
              />

              <FormInput
                type={inputTypes.CHECK}
                name="removeFailedDownloads"
                {...removeFailedDownloads}
                onChange={handleInputChange}
              />
            </FormRow>
          </ModalSection>
        </Form>
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteDownloadClientPress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <AdvancedSettingsButton showLabel={false} />

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={handleTestPress}
        >
          {translate('Test')}
        </SpinnerErrorButton>

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

export default EditDownloadClientModalContent;
