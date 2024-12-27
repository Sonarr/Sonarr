import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { AutoTaggingSpecificationAppState } from 'App/State/SettingsAppState';
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
import { inputTypes, kinds } from 'Helpers/Props';
import {
  clearAutoTaggingSpecificationPending,
  saveAutoTaggingSpecification,
  setAutoTaggingSpecificationFieldValue,
  setAutoTaggingSpecificationValue,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import { AutoTaggingSpecification } from 'typings/AutoTagging';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditSpecificationModalContent.css';

export interface EditSpecificationModalContentProps {
  id?: number;
  onDeleteSpecificationPress?: () => void;
  onModalClose: () => void;
}

function EditSpecificationModalContent({
  id,
  onDeleteSpecificationPress,
  onModalClose,
}: EditSpecificationModalContentProps) {
  const advancedSettings = useSelector(
    (state: AppState) => state.settings.advancedSettings
  );

  const { item, ...otherFormProps } = useSelector(
    createProviderSettingsSelectorHook<
      AutoTaggingSpecification,
      AutoTaggingSpecificationAppState
    >('autoTaggingSpecifications', id)
  );

  const dispatch = useDispatch();

  const onInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setAutoTaggingSpecificationValue({ name, value }));
    },
    [dispatch]
  );

  const onFieldChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setAutoTaggingSpecificationFieldValue({ name, value }));
    },
    [dispatch]
  );

  const onCancelPress = useCallback(() => {
    dispatch(clearAutoTaggingSpecificationPending());
    onModalClose();
  }, [dispatch, onModalClose]);

  const onSavePress = useCallback(() => {
    dispatch(saveAutoTaggingSpecification({ id }));
    onModalClose();
  }, [dispatch, id, onModalClose]);

  const { implementationName, name, negate, required, fields } = item;

  return (
    <ModalContent onModalClose={onCancelPress}>
      <ModalHeader>
        {id
          ? translate('EditConditionImplementation', { implementationName })
          : translate('AddConditionImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        <Form {...otherFormProps}>
          {fields && fields.some((x) => x.label === 'Regular Expression') && (
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
          )}

          <FormGroup>
            <FormLabel>{translate('Name')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT}
              name="name"
              {...name}
              onChange={onInputChange}
            />
          </FormGroup>

          {fields &&
            fields.map((field) => {
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
            })}

          <FormGroup>
            <FormLabel>{translate('Negate')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="negate"
              {...negate}
              helpText={translate('AutoTaggingNegateHelpText', {
                implementationName,
              })}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Required')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="required"
              {...required}
              helpText={translate('AutoTaggingRequiredHelpText', {
                implementationName,
              })}
              onChange={onInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteSpecificationPress}
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
