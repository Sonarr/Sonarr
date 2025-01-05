import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { CustomFormatSpecificationAppState } from 'App/State/SettingsAppState';
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
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds } from 'Helpers/Props';
import {
  saveCustomFormatSpecification,
  setCustomFormatSpecificationFieldValue,
  setCustomFormatSpecificationValue,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import CustomFormatSpecification from 'typings/CustomFormatSpecification';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditSpecificationModalContent.css';

export interface EditSpecificationModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteSpecificationPress?: () => void;
}

function EditSpecificationModalContent({
  id,
  onModalClose,
  onDeleteSpecificationPress,
}: EditSpecificationModalContentProps) {
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();

  const { item, validationErrors, validationWarnings } = useSelector(
    createProviderSettingsSelectorHook<
      CustomFormatSpecification,
      CustomFormatSpecificationAppState
    >('customFormatSpecifications', id)
  );

  const { implementationName, name, negate, required, fields } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setCustomFormatSpecificationValue(change));
    },
    [dispatch]
  );

  const handleFieldChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setCustomFormatSpecificationFieldValue(change));
    },
    [dispatch]
  );

  const handleSavePress = useCallback(() => {
    dispatch(saveCustomFormatSpecification({ id }));

    onModalClose();
  }, [id, dispatch, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
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
              onChange={handleInputChange}
            />
          </FormGroup>

          {fields
            ? fields.map((field) => {
                return (
                  <ProviderFieldFormGroup
                    key={field.name}
                    advancedSettings={showAdvancedSettings}
                    provider="specifications"
                    providerData={item}
                    {...field}
                    onChange={handleFieldChange}
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
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Required')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="required"
              {...required}
              helpText={translate('RequiredHelpText', { implementationName })}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {id && (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteSpecificationPress}
          >
            {translate('Delete')}
          </Button>
        )}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton isSpinning={false} onPress={handleSavePress}>
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditSpecificationModalContent;
