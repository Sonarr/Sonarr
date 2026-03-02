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
import { inputTypes, kinds } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { useManageExternalDecision } from 'Settings/ExternalDecisions/useExternalDecisions';
import { SelectedSchema } from 'Settings/useProviderSchema';
import { EnhancedSelectInputChanged, InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditExternalDecisionModalContent.css';

export interface EditExternalDecisionModalContentProps {
  id?: number;
  selectedSchema?: SelectedSchema;
  onModalClose: () => void;
  onDeleteExternalDecisionPress?: () => void;
}

function EditExternalDecisionModalContent({
  id,
  selectedSchema,
  onModalClose,
  onDeleteExternalDecisionPress,
}: EditExternalDecisionModalContentProps) {
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
  } = useManageExternalDecision(id, selectedSchema);

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName,
    name,
    fields,
    tags,
    message,
    decisionType,
    enable,
    priority,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - change is not yet typed
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

  const handleTestPress = useCallback(() => {
    testProvider();
  }, [testProvider]);

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
        {id
          ? translate('EditExternalDecisionImplementation', {
              implementationName,
            })
          : translate('AddExternalDecisionImplementation', {
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

          <FormGroup>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Enable')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enable"
              {...enable}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('DecisionType')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="decisionType"
              values={[
                {
                  key: 'rejection',
                  value: translate('ExternalDecisionTypeRejection'),
                },
                {
                  key: 'prioritization',
                  value: translate('ExternalDecisionTypePrioritization'),
                },
              ]}
              helpText={translate('ExternalDecisionTypeHelpText')}
              {...decisionType}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('ExternalDecisionPriority')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="priority"
              helpText={translate('ExternalDecisionPriorityHelpText')}
              min={1}
              max={50}
              {...priority}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText={translate('ExternalDecisionTagsHelpText')}
              {...tags}
              onChange={handleInputChange}
            />
          </FormGroup>

          {fields.map((field) => {
            return (
              <ProviderFieldFormGroup
                key={field.name}
                {...field}
                advancedSettings={showAdvancedSettings}
                provider="externalDecision"
                providerData={item}
                onChange={handleFieldChange}
              />
            );
          })}
        </Form>
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteExternalDecisionPress}
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

export default EditExternalDecisionModalContent;
