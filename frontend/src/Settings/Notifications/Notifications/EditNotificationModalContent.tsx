import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { NotificationAppState } from 'App/State/SettingsAppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import {
  saveNotification,
  setNotificationFieldValues,
  setNotificationValue,
  testNotification,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import { EnhancedSelectInputChanged, InputChanged } from 'typings/inputs';
import Notification from 'typings/Notification';
import translate from 'Utilities/String/translate';
import NotificationEventItems from './NotificationEventItems';
import styles from './EditNotificationModalContent.css';

export interface EditNotificationModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteNotificationPress?: () => void;
}

function EditNotificationModalContent({
  id,
  onModalClose,
  onDeleteNotificationPress,
}: EditNotificationModalContentProps) {
  const dispatch = useDispatch();
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    isFetching,
    error,
    isSaving,
    isTesting = false,
    saveError,
    item,
    validationErrors,
    validationWarnings,
  } = useSelector(
    createProviderSettingsSelectorHook<Notification, NotificationAppState>(
      'notifications',
      id
    )
  );

  const wasSaving = usePrevious(isSaving);

  const { implementationName, name, fields, tags, message } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setNotificationValue(change));
    },
    [dispatch]
  );

  const handleFieldChange = useCallback(
    ({
      name,
      value,
      additionalProperties,
    }: EnhancedSelectInputChanged<unknown>) => {
      dispatch(
        // @ts-expect-error - actions are not typed
        setNotificationFieldValues({
          properties: { [name]: value, ...additionalProperties },
        })
      );
    },
    [dispatch]
  );

  const handleTestPress = useCallback(() => {
    dispatch(testNotification({ id }));
  }, [id, dispatch]);

  const handleSavePress = useCallback(() => {
    dispatch(saveNotification({ id }));
  }, [id, dispatch]);

  useEffect(() => {
    if (wasSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
          ? translate('EditConnectionImplementation', { implementationName })
          : translate('AddConnectionImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>{translate('AddNotificationError')}</Alert>
        ) : null}

        {!isFetching && !error ? (
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

            <NotificationEventItems
              item={item}
              onInputChange={handleInputChange}
            />

            <FormGroup>
              <FormLabel>{translate('Tags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TAG}
                name="tags"
                helpText={translate('NotificationsTagsSeriesHelpText')}
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
                  provider="notification"
                  providerData={item}
                  onChange={handleFieldChange}
                />
              );
            })}
          </Form>
        ) : null}
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteNotificationPress}
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

export default EditNotificationModalContent;
