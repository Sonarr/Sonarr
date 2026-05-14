import React, { useMemo } from 'react';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import Field from 'typings/Field';
import translate from 'Utilities/String/translate';
import { useCustomFormat, useCustomFormats } from './useCustomFormats';
import styles from './ExportCustomFormatModalContent.css';

const omittedProperties = ['id', 'implementationName', 'infoLink'];

function replacer(key: string, value: unknown) {
  if (omittedProperties.includes(key)) {
    return undefined;
  }

  if (key === 'fields') {
    return (value as Field[]).reduce<Record<string, unknown>>((acc, cur) => {
      acc[cur.name] = cur.value;

      return acc;
    }, {});
  }

  return value;
}

export interface ExportCustomFormatModalContentProps {
  id: number;
  onModalClose: () => void;
}

function ExportCustomFormatModalContent({
  id,
  onModalClose,
}: ExportCustomFormatModalContentProps) {
  const { isLoading, error } = useCustomFormats();
  const customFormat = useCustomFormat(id);

  const json = useMemo(() => {
    return customFormat ? JSON.stringify(customFormat, replacer, 2) : '';
  }, [customFormat]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ExportCustomFormat')}</ModalHeader>

      <ModalBody>
        <div>
          {isLoading ? <LoadingIndicator /> : null}

          {!isLoading && error ? (
            <Alert kind={kinds.DANGER}>
              {translate('CustomFormatsLoadError')}
            </Alert>
          ) : null}

          {!isLoading && !error ? (
            <div>
              <pre>{json}</pre>
            </div>
          ) : null}
        </div>
      </ModalBody>

      <ModalFooter>
        <ClipboardButton
          className={styles.button}
          value={json}
          title={translate('CopyToClipboard')}
          kind={kinds.DEFAULT}
        />
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default ExportCustomFormatModalContent;
