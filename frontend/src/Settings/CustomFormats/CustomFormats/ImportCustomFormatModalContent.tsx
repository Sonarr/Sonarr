import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  clearCustomFormatSpecificationPending,
  deleteAllCustomFormatSpecification,
  fetchCustomFormatSpecificationSchema,
  saveCustomFormatSpecification,
  selectCustomFormatSpecificationSchema,
  setCustomFormatSpecificationFieldValue,
  setCustomFormatSpecificationValue,
  setCustomFormatValue,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import CustomFormatSpecification from 'typings/CustomFormatSpecification';
import Field from 'typings/Field';
import { InputChanged } from 'typings/inputs';
import { ValidationError } from 'typings/pending';
import translate from 'Utilities/String/translate';
import styles from './ImportCustomFormatModalContent.css';

interface ImportCustomFormatModalContentProps {
  onModalClose: () => void;
}

function ImportCustomFormatModalContent({
  onModalClose,
}: ImportCustomFormatModalContentProps) {
  const dispatch = useDispatch();

  const { isFetching, error } = useSelector(
    createProviderSettingsSelectorHook('customFormats', undefined)
  );

  const {
    isPopulated: isSpecificationsPopulated,
    schema: specificationsSchema,
  } = useSelector(
    (state: AppState) => state.settings.customFormatSpecifications
  );

  const importTimeout = useRef<ReturnType<typeof setTimeout>>();
  const [json, setJson] = useState('');
  const [isSpinning, setIsSpinning] = useState(false);
  const [parseError, setParseError] = useState<ValidationError>();

  const handleChange = useCallback(({ value }: InputChanged<string>) => {
    setJson(value);
  }, []);

  const clearPending = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'settings.customFormats' }));
    dispatch(clearCustomFormatSpecificationPending());
    dispatch(deleteAllCustomFormatSpecification());
  }, [dispatch]);

  const parseFields = useCallback(
    (fields: Field[], schema: CustomFormatSpecification) => {
      for (const [key, value] of Object.entries(fields)) {
        const field = schema.fields.find((field) => field.name === key);
        if (!field) {
          throw new Error(
            translate('CustomFormatUnknownConditionOption', {
              key,
              implementation: schema.implementationName,
            })
          );
        }

        // @ts-expect-error - actions are not typed
        dispatch(setCustomFormatSpecificationFieldValue({ name: key, value }));
      }
    },
    [dispatch]
  );

  const parseSpecification = useCallback(
    (spec: CustomFormatSpecification) => {
      const selectedImplementation = specificationsSchema.find((s) => {
        return s.implementation === spec.implementation;
      });

      if (!selectedImplementation) {
        throw new Error(
          translate('CustomFormatUnknownCondition', {
            implementation: spec.implementation,
          })
        );
      }

      dispatch(
        selectCustomFormatSpecificationSchema({
          implementation: spec.implementation,
        })
      );

      for (const [key, value] of Object.entries(spec)) {
        if (key === 'fields') {
          parseFields(value, selectedImplementation);
        } else if (key !== 'id') {
          // @ts-expect-error - actions are not typed
          dispatch(setCustomFormatSpecificationValue({ name: key, value }));
        }
      }

      dispatch(saveCustomFormatSpecification());
    },
    [specificationsSchema, dispatch, parseFields]
  );

  const handleImportPress = useCallback(() => {
    setIsSpinning(true);

    importTimeout.current = setTimeout(() => {
      clearPending();

      try {
        const cf = JSON.parse(json);

        for (const [key, value] of Object.entries(cf)) {
          if (key === 'specifications') {
            for (const spec of value as CustomFormatSpecification[]) {
              parseSpecification(spec);
            }
          } else if (key !== 'id') {
            // @ts-expect-error - actions are not typed
            dispatch(setCustomFormatValue({ name: key, value }));
          }
        }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } catch (err: any) {
        clearPending();

        setParseError({
          isWarning: false,
          errorMessage: err.message,
          detailedDescription: err.stack,
          propertyName: 'customFormatJson',
          severity: 'error',
        });

        return;
      }

      onModalClose();
    }, 250);
  }, [json, clearPending, dispatch, parseSpecification, onModalClose]);

  useEffect(() => {
    dispatch(fetchCustomFormatSpecificationSchema());
  }, [dispatch]);

  useEffect(() => {
    return () => {
      if (importTimeout.current) {
        // eslint-disable-next-line react-hooks/exhaustive-deps
        clearTimeout(importTimeout.current);
      }
    };
  }, []);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ImportCustomFormat')}</ModalHeader>

      <ModalBody>
        <div>
          {isFetching ? <LoadingIndicator /> : null}

          {!isFetching && error ? (
            <Alert kind={kinds.DANGER}>
              {translate('CustomFormatsLoadError')}
            </Alert>
          ) : null}

          {!isFetching && !error && isSpecificationsPopulated ? (
            <Form>
              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>{translate('CustomFormatJson')}</FormLabel>
                <FormInputGroup
                  key={0}
                  inputClassName={styles.input}
                  type={inputTypes.TEXT_AREA}
                  name="customFormatJson"
                  value={json}
                  placeholder={'{\n  "name": "Custom Format"\n}'}
                  errors={parseError ? [parseError] : []}
                  onChange={handleChange}
                />
              </FormGroup>
            </Form>
          ) : null}
        </div>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>
        <SpinnerErrorButton
          isSpinning={isSpinning}
          error={parseError?.errorMessage}
          onPress={handleImportPress}
        >
          {translate('Import')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default ImportCustomFormatModalContent;
