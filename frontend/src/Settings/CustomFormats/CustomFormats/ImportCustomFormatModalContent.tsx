import React, { useCallback, useRef, useState } from 'react';
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
import Field from 'typings/Field';
import { InputChanged } from 'typings/inputs';
import { ValidationError } from 'typings/pending';
import translate from 'Utilities/String/translate';
import {
  CustomFormat,
  CustomFormatSpecification,
  useCustomFormatSchema,
} from './useCustomFormats';
import styles from './ImportCustomFormatModalContent.css';

type SchemaItem = CustomFormatSpecification & {
  presets?: CustomFormatSpecification[];
};

interface ImportCustomFormatModalContentProps {
  onImport: (customFormat: CustomFormat) => void;
  onModalClose: () => void;
}

interface RawSpec {
  name: string;
  implementation: string;
  negate?: boolean;
  required?: boolean;
  fields?: Record<string, unknown>;
}

interface RawCustomFormat {
  name?: string;
  includeCustomFormatWhenRenaming?: boolean;
  specifications?: RawSpec[];
}

function ImportCustomFormatModalContent({
  onImport,
  onModalClose,
}: ImportCustomFormatModalContentProps) {
  const schemaResult = useCustomFormatSchema();
  const schema = schemaResult.schema as SchemaItem[];
  const { isSchemaLoading, schemaError } = schemaResult;

  const importTimeout = useRef<ReturnType<typeof setTimeout>>();
  const [json, setJson] = useState('');
  const [isSpinning, setIsSpinning] = useState(false);
  const [parseError, setParseError] = useState<ValidationError>();

  const handleChange = useCallback(({ value }: InputChanged<string>) => {
    setJson(value);
    setParseError(undefined);
  }, []);

  const buildSpec = useCallback(
    (raw: RawSpec): CustomFormatSpecification => {
      const schemaSpec = schema.find(
        (s) => s.implementation === raw.implementation
      );

      if (!schemaSpec) {
        throw new Error(
          translate('CustomFormatUnknownCondition', {
            implementation: raw.implementation,
          })
        );
      }

      const fields: Field[] = schemaSpec.fields.map((f) => ({ ...f }));

      if (raw.fields) {
        for (const [key, value] of Object.entries(raw.fields)) {
          const target = fields.find((f) => f.name === key);

          if (!target) {
            throw new Error(
              translate('CustomFormatUnknownConditionOption', {
                key,
                implementation: schemaSpec.implementationName,
              })
            );
          }

          target.value = value as Field['value'];
        }
      }

      return {
        id: 0,
        name: raw.name,
        implementation: schemaSpec.implementation,
        implementationName: schemaSpec.implementationName,
        infoLink: schemaSpec.infoLink,
        negate: raw.negate ?? false,
        required: raw.required ?? false,
        fields,
      };
    },
    [schema]
  );

  const handleImportPress = useCallback(() => {
    setIsSpinning(true);

    importTimeout.current = setTimeout(() => {
      try {
        const parsed = JSON.parse(json) as RawCustomFormat;

        const specifications = (parsed.specifications ?? []).map(buildSpec);

        const customFormat: CustomFormat = {
          id: 0,
          name: parsed.name ?? '',
          includeCustomFormatWhenRenaming:
            parsed.includeCustomFormatWhenRenaming ?? false,
          specifications,
        };

        onImport(customFormat);
        onModalClose();
      } catch (err) {
        const message = err instanceof Error ? err.message : String(err);
        const stack = err instanceof Error ? err.stack : undefined;

        setParseError({
          isWarning: false,
          errorMessage: message,
          detailedDescription: stack,
          propertyName: 'customFormatJson',
          severity: 'error',
        });
      } finally {
        setIsSpinning(false);
      }
    }, 250);
  }, [json, buildSpec, onImport, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ImportCustomFormat')}</ModalHeader>

      <ModalBody>
        <div>
          {isSchemaLoading ? <LoadingIndicator /> : null}

          {!isSchemaLoading && schemaError ? (
            <Alert kind={kinds.DANGER}>
              {translate('CustomFormatsLoadError')}
            </Alert>
          ) : null}

          {!isSchemaLoading && !schemaError ? (
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
