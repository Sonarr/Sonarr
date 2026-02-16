import React, { useCallback, useEffect } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { SelectedSchema } from 'Settings/useProviderSchema';
import { EnhancedSelectInputChanged, InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import { useManageIndexer } from '../useIndexers';
import styles from './EditIndexerModalContent.css';

export interface EditIndexerModalContentProps {
  id?: number;
  cloneId?: number;
  selectedSchema?: SelectedSchema;
  onModalClose: () => void;
  onDeleteIndexerPress?: () => void;
}

function EditIndexerModalContent({
  id,
  cloneId,
  selectedSchema,
  onModalClose,
  onDeleteIndexerPress,
}: EditIndexerModalContentProps) {
  const showAdvancedSettings = useShowAdvancedSettings();

  const {
    item,
    updateFieldValue,
    updateValue,
    saveProvider,
    isSaving,
    saveError,
    testProvider,
    isTesting,
    validationErrors,
    validationWarnings,
  } = useManageIndexer(id, cloneId, selectedSchema);

  const wasSaving = usePrevious(isSaving);

  const {
    implementationName = '',
    name,
    enableRss,
    enableAutomaticSearch,
    enableInteractiveSearch,
    supportsRss,
    supportsSearch,
    tags,
    fields,
    priority,
    seasonSearchMaximumSingleEpisodeAge,
    protocol,
    downloadClientId,
  } = item;

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - InputChanged is not typed correctly
      updateValue(change.name, change.value);
    },
    [updateValue]
  );

  const handleFieldChange = useCallback(
    ({
      name,
      value,
      additionalProperties,
    }: EnhancedSelectInputChanged<unknown>) => {
      updateFieldValue({ [name]: value, ...additionalProperties });
    },
    [updateFieldValue]
  );

  const handleSavePress = useCallback(() => {
    saveProvider();
  }, [saveProvider]);

  const handleTestPress = useCallback(() => {
    testProvider();
  }, [testProvider]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
          ? translate('EditIndexerImplementation', { implementationName })
          : translate('AddIndexerImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        <Form
          validationErrors={validationErrors}
          validationWarnings={validationWarnings}
        >
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
            <FormLabel>{translate('EnableRss')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enableRss"
              helpText={
                supportsRss.value ? translate('EnableRssHelpText') : undefined
              }
              helpTextWarning={
                supportsRss.value
                  ? undefined
                  : translate('RssIsNotSupportedWithThisIndexer')
              }
              isDisabled={!supportsRss.value}
              {...enableRss}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('EnableAutomaticSearch')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enableAutomaticSearch"
              helpText={
                supportsSearch.value
                  ? translate('EnableAutomaticSearchHelpText')
                  : undefined
              }
              helpTextWarning={
                supportsSearch.value
                  ? undefined
                  : translate('SearchIsNotSupportedWithThisIndexer')
              }
              isDisabled={!supportsSearch.value}
              {...enableAutomaticSearch}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('EnableInteractiveSearch')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="enableInteractiveSearch"
              helpText={
                supportsSearch.value
                  ? translate('EnableInteractiveSearchHelpText')
                  : undefined
              }
              helpTextWarning={
                supportsSearch.value
                  ? undefined
                  : translate('SearchIsNotSupportedWithThisIndexer')
              }
              isDisabled={!supportsSearch.value}
              {...enableInteractiveSearch}
              onChange={handleInputChange}
            />
          </FormGroup>

          {fields?.map((field) => {
            return (
              <ProviderFieldFormGroup
                key={field.name}
                advancedSettings={showAdvancedSettings}
                provider="indexer"
                providerData={item}
                {...field}
                onChange={handleFieldChange}
              />
            );
          })}

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('IndexerPriority')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="priority"
              helpText={translate('IndexerPriorityHelpText')}
              min={1}
              max={50}
              {...priority}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('MaximumSingleEpisodeAge')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="seasonSearchMaximumSingleEpisodeAge"
              helpText={translate('MaximumSingleEpisodeAgeHelpText')}
              min={0}
              unit="days"
              {...seasonSearchMaximumSingleEpisodeAge}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('DownloadClient')}</FormLabel>

            <FormInputGroup
              type={inputTypes.DOWNLOAD_CLIENT_SELECT}
              name="downloadClientId"
              helpText={translate('IndexerDownloadClientHelpText')}
              {...downloadClientId}
              includeAny={true}
              protocol={protocol.value}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText={translate('IndexerTagSeriesHelpText')}
              {...tags}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        {id ? (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteIndexerPress}
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

export default EditIndexerModalContent;
