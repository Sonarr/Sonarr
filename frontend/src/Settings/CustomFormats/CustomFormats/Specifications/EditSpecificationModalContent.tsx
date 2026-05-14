import React, { useCallback, useMemo } from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
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
import selectSettings from 'Store/Selectors/selectSettings';
import { InputChanged } from 'typings/inputs';
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

  const onInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      setPendingChange(
        name as keyof CustomFormatSpecification,
        value as CustomFormatSpecification[keyof CustomFormatSpecification]
      );
    },
    [setPendingChange]
  );

  const onFieldChange = useCallback(
    ({ name, value }: InputChanged) => {
      setPendingField(name, value);
    },
    [setPendingField]
  );

  const onDeletePress = useCallback(() => {
    if (onDeleteSpecificationPress) {
      onDeleteSpecificationPress();
    }
  }, [onDeleteSpecificationPress]);

  const onCancelPress = useCallback(() => {
    clearPendingChanges();
    clearPendingFields();
    onModalClose();
  }, [clearPendingChanges, clearPendingFields, onModalClose]);

  const onSavePress = useCallback(() => {
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
    <ModalContent onModalClose={onCancelPress}>
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
            <Alert kind={kinds.INFO}>
              <div>
                <InlineMarkdown
                  data={translate('ConditionUsingRegularExpressions')}
                />
              </div>
              <div>
                <InlineMarkdown
                  data={translate('RegularExpressionsTutorialLink', {
                    url: 'https://www.regular-expressions.info/tutorial.html',
                  })}
                />
              </div>
              <div>
                <InlineMarkdown
                  data={translate('RegularExpressionsCanBeTested', {
                    url: 'http://regexstorm.net/tester',
                  })}
                />
              </div>
            </Alert>
          ) : null}

          <FormGroup>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={onInputChange}
            />
          </FormGroup>

          {fields
            ? fields.map((field) => {
                return (
                  <ProviderFieldFormGroup
                    key={field.name}
                    advancedSettings={advancedSettings}
                    provider="specifications"
                    providerData={item}
                    {...field}
                    onChange={onFieldChange}
                  />
                );
              })
            : null}

          <FormGroup>
            <FormLabel>{translate('Negate')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="negate"
              {...negate}
              helpText={translate('NegateHelpText', { implementationName })}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Required')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="required"
              {...required}
              helpText={translate('RequiredHelpText', { implementationName })}
              onChange={onInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {specification.id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeletePress}
          >
            {translate('Delete')}
          </Button>
        ) : null}

        <Button onPress={onCancelPress}>{translate('Cancel')}</Button>

        <SpinnerErrorButton isSpinning={false} onPress={onSavePress}>
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditSpecificationModalContent;
