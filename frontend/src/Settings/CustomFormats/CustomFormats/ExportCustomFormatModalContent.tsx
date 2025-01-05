import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import ClipboardButton from 'Components/Link/ClipboardButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import { fetchCustomFormatSpecifications } from 'Store/Actions/settingsActions';
import Field from 'typings/Field';
import translate from 'Utilities/String/translate';
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

function createCustomFormatJsonSelector(id: number) {
  return createSelector(
    (state: AppState) => state.settings.customFormats,
    (customFormats) => {
      const customFormat = customFormats.items.find((i) => i.id === id);

      const json = customFormat
        ? JSON.stringify(customFormat, replacer, 2)
        : '';

      return json;
    }
  );
}

export interface ExportCustomFormatModalContentProps {
  id: number;
  onModalClose: () => void;
}

function ExportCustomFormatModalContent({
  id,
  onModalClose,
}: ExportCustomFormatModalContentProps) {
  const dispatch = useDispatch();

  const { isFetching, error } = useSelector(
    (state: AppState) => state.settings.customFormats
  );

  const json = useSelector(createCustomFormatJsonSelector(id));

  useEffect(() => {
    dispatch(fetchCustomFormatSpecifications({ id }));
  }, [id, dispatch]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('ExportCustomFormat')}</ModalHeader>

      <ModalBody>
        <div>
          {isFetching ? <LoadingIndicator /> : null}

          {!isFetching && error ? (
            <Alert kind={kinds.DANGER}>
              {translate('CustomFormatsLoadError')}
            </Alert>
          ) : null}

          {!isFetching && !error ? (
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
