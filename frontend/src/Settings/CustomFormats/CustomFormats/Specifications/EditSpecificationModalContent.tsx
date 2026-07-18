import React, { useCallback, useMemo } from 'react';
import Form from 'Components/Form/Form';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { usePendingChangesStore } from 'Helpers/Hooks/usePendingChangesStore';
import { usePendingFieldsStore } from 'Helpers/Hooks/usePendingFieldsStore';
import { inputTypes, kinds } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { InputChanged } from 'typings/inputs';
import selectSettings from 'Utilities/selectSettings';
import translate from 'Utilities/String/translate';
import { CustomFormatSpecification } from '../useCustomFormats';
import styles from './EditSpecificationModalContent.css';

export interface EditSpecificationModalContentProps {
  specification: CustomFormatSpecification;
  onSave: (spec: CustomFormatSpecification) => void;
  onDeleteSpecificationPress?: () => void;
  onModalClose: () => void;
}

function EditSpecificationModalContent({
  specification,
  onSave,
  onDeleteSpecificationPress,
  onModalClose,
}: EditSpecificationModalContentProps) {
  const advancedSettings = useShowAdvancedSettings();

  const { pendingChanges, setPendingChange, clearPendingChanges } =
    usePendingChangesStore<CustomFormatSpecification>({});

  const {
    pendingFields,
    setPendingField,
    hasPendingFields,
    clearPendingFields,
  } = usePendingFieldsStore();

  const {
    settings: item,
    validationErrors,
    validationWarnings,
  } = useMemo(() => {
    const combinedPendingChanges = hasPendingFields
      ? {
          ...pendingChanges,
          fields: Object.fromEntries(pendingFields),
        }
      : pendingChanges;

    return selectSettings(specification, combinedPendingChanges);
  }, [specification, pendingChanges, pendingFields, hasPendingFields]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      setPendingChange(
        name as keyof CustomFormatSpecification,
        value as CustomFormatSpecification[keyof CustomFormatSpecification]
      );
    },
    [setPendingChange]
  );

  const handleFieldChange = useCallback(
    ({ name, value }: InputChanged) => {
      setPendingField(name, value);
    },
    [setPendingField]
  );

  const handleCancelPress = useCallback(() => {
    clearPendingChanges();
    clearPendingFields();
    onModalClose();
  }, [clearPendingChanges, clearPendingFields, onModalClose]);

  const handleSavePress = useCallback(() => {
    let updatedSpec: CustomFormatSpecification = {
      ...specification,
      ...pendingChanges,
    };

    if (hasPendingFields) {
      updatedSpec = {
        ...updatedSpec,
        fields: specification.fields.map((f) =>
          pendingFields.has(f.name)
            ? { ...f, value: pendingFields.get(f.name) as typeof f.value }
            : f
        ),
      };
    }

    onSave(updatedSpec);
    onModalClose();
  }, [
    specification,
    pendingChanges,
    pendingFields,
    hasPendingFields,
    onSave,
    onModalClose,
  ]);

  const { implementationName, name, negate, required, fields } = item;

  return (
    <ModalContent onModalClose={handleCancelPress}>
      <ModalHeader>
        {specification.id
          ? translate('EditConditionImplementation', { implementationName })
          : translate('AddConditionImplementation', { implementationName })}
      </ModalHeader>
      <ModalBody>
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
          {fields?.some(
            (x) =>
              x.label ===
              translate('CustomFormatsSpecificationRegularExpression')
          ) ? (
            <p className={styles.intro}>
              <InlineMarkdown
                data={translate('ConditionUsingRegularExpressions')}
              />{' '}
              <InlineMarkdown
                data={translate('RegularExpressionsTutorialLink', {
                  url: 'https://www.regular-expressions.info/tutorial.html',
                })}
              />{' '}
              <InlineMarkdown
                data={translate('RegularExpressionsCanBeTested', {
                  url: 'http://regexstorm.net/tester',
                })}
              />
            </p>
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

          {fields
            ? fields.map((field) => {
                return (
                  <ProviderFieldFormGroup
                    key={field.name}
                    advancedSettings={advancedSettings}
                    layout="row"
                    provider="specifications"
                    providerData={item}
                    {...field}
                    onChange={handleFieldChange}
                  />
                );
              })
            : null}

          <FormRow>
            <FormLabel>{translate('Negate')}</FormLabel>
            <FormInputHelpText
              text={translate('NegateHelpText', { implementationName })}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="negate"
              {...negate}
              onChange={handleInputChange}
            />
          </FormRow>

          <FormRow>
            <FormLabel>{translate('Required')}</FormLabel>
            <FormInputHelpText
              text={translate('RequiredHelpText', { implementationName })}
            />
            <FormInput
              type={inputTypes.CHECK}
              name="required"
              {...required}
              onChange={handleInputChange}
            />
          </FormRow>
        </Form>
      </ModalBody>
      <ModalFooter>
        {specification.id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteSpecificationPress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <Button onPress={handleCancelPress}>{translate('Cancel')}</Button>

        <SpinnerErrorButton isSpinning={false} onPress={handleSavePress}>
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditSpecificationModalContent;
