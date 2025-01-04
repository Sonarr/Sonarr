import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { DownloadClientAppState } from 'App/State/SettingsAppState';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
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
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import {
  saveDownloadClient,
  setDownloadClientFieldValue,
  setDownloadClientValue,
  testDownloadClient,
} from 'Store/Actions/settingsActions';
import { createProviderSettingsSelectorHook } from 'Store/Selectors/createProviderSettingsSelector';
import DownloadClient from 'typings/DownloadClient';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './EditDownloadClientModalContent.css';

export interface EditDownloadClientModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteDownloadClientPress?: () => void;
}

function EditDownloadClientModalContent({
  id,
  onModalClose,
  onDeleteDownloadClientPress,
}: EditDownloadClientModalContentProps) {
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
    createProviderSettingsSelectorHook<DownloadClient, DownloadClientAppState>(
      'downloadClients',
      id
    )
  );

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName,
    name,
    enable,
    protocol,
    priority,
    removeCompletedDownloads,
    removeFailedDownloads,
    fields,
    tags,
    message,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setDownloadClientValue(change));
    },
    [dispatch]
  );

  const handleFieldChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions are not typed
      dispatch(setDownloadClientFieldValue(change));
    },
    [dispatch]
  );

  const handleTestPress = useCallback(() => {
    dispatch(testDownloadClient({ id }));
  }, [id, dispatch]);

  const handleSavePress = useCallback(() => {
    dispatch(saveDownloadClient({ id }));
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
          ? translate('EditDownloadClientImplementation', {
              implementationName,
            })
          : translate('AddDownloadClientImplementation', {
              implementationName,
            })}
      </ModalHeader>

      <ModalBody>
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <Alert kind={kinds.DANGER}>
            {translate('AddDownloadClientError')}
          </Alert>
        ) : null}

        {!isFetching && !error ? (
          <Form
            validationErrors={validationErrors}
            validationWarnings={validationWarnings}
          >
            {!!message && (
              <Alert className={styles.message} kind={message.value.type}>
                {message.value.message}
              </Alert>
            )}

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

            {fields.map((field) => {
              return (
                <ProviderFieldFormGroup
                  key={field.name}
                  advancedSettings={showAdvancedSettings}
                  provider="downloadClient"
                  providerData={item}
                  {...field}
                  onChange={handleFieldChange}
                />
              );
            })}

            <FormGroup
              advancedSettings={showAdvancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('ClientPriority')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="priority"
                helpText={translate('DownloadClientPriorityHelpText')}
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
                helpText={translate('DownloadClientSeriesTagHelpText')}
                {...tags}
                onChange={handleInputChange}
              />
            </FormGroup>

            <FieldSet
              size={sizes.SMALL}
              legend={translate('CompletedDownloadHandling')}
            >
              <FormGroup>
                <FormLabel>{translate('RemoveCompleted')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="removeCompletedDownloads"
                  helpText={translate('RemoveCompletedDownloadsHelpText')}
                  {...removeCompletedDownloads}
                  onChange={handleInputChange}
                />
              </FormGroup>

              {protocol.value === 'torrent' ? null : (
                <FormGroup>
                  <FormLabel>{translate('RemoveFailed')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="removeFailedDownloads"
                    helpText={translate('RemoveFailedDownloadsHelpText')}
                    {...removeFailedDownloads}
                    onChange={handleInputChange}
                  />
                </FormGroup>
              )}
            </FieldSet>
          </Form>
        ) : null}
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteDownloadClientPress}
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

export default EditDownloadClientModalContent;
