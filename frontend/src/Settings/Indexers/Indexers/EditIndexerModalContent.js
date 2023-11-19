import PropTypes from 'prop-types';
import React from 'react';
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
import { inputTypes, kinds } from 'Helpers/Props';
import AdvancedSettingsButton from 'Settings/AdvancedSettingsButton';
import translate from 'Utilities/String/translate';
import styles from './EditIndexerModalContent.css';

function EditIndexerModalContent(props) {
  const {
    advancedSettings,
    isFetching,
    error,
    isSaving,
    isTesting,
    saveError,
    item,
    onInputChange,
    onFieldChange,
    onModalClose,
    onSavePress,
    onTestPress,
    onDeleteIndexerPress,
    onAdvancedSettingsPress,
    ...otherProps
  } = props;

  const {
    id,
    implementationName,
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
    downloadClientId
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditIndexerImplementation', { implementationName }) : translate('AddIndexerImplementation', { implementationName })}
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>
              {translate('AddIndexerError')}
            </div>
        }

        {
          !isFetching && !error &&
            <Form {...otherProps}>
              <FormGroup>
                <FormLabel>{translate('Name')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="name"
                  {...name}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('EnableRss')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableRss"
                  helpText={supportsRss.value ? translate('EnableRssHelpText') : undefined}
                  helpTextWarning={supportsRss.value ? undefined : translate('RssIsNotSupportedWithThisIndexer')}
                  isDisabled={!supportsRss.value}
                  {...enableRss}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('EnableAutomaticSearch')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableAutomaticSearch"
                  helpText={supportsSearch.value ? translate('EnableAutomaticSearchHelpText') : undefined}
                  helpTextWarning={supportsSearch.value ? undefined : translate('SearchIsNotSupportedWithThisIndexer')}
                  isDisabled={!supportsSearch.value}
                  {...enableAutomaticSearch}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('EnableInteractiveSearch')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableInteractiveSearch"
                  helpText={supportsSearch.value ? translate('EnableInteractiveSearchHelpText') : undefined}
                  helpTextWarning={supportsSearch.value ? undefined : translate('SearchIsNotSupportedWithThisIndexer')}
                  isDisabled={!supportsSearch.value}
                  {...enableInteractiveSearch}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                fields.map((field) => {
                  return (
                    <ProviderFieldFormGroup
                      key={field.name}
                      advancedSettings={advancedSettings}
                      provider="indexer"
                      providerData={item}
                      {...field}
                      onChange={onFieldChange}
                    />
                  );
                })
              }

              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('IndexerPriority')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="priority"
                  helpText={translate('IndexerPriorityHelpText')}
                  min={1}
                  max={50}
                  {...priority}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('MaximumSingleEpisodeAge')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="seasonSearchMaximumSingleEpisodeAge"
                  helpText={translate('MaximumSingleEpisodeAgeHelpText')}
                  min={0}
                  unit="days"
                  {...seasonSearchMaximumSingleEpisodeAge}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('DownloadClient')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.DOWNLOAD_CLIENT_SELECT}
                  name="downloadClientId"
                  helpText={translate('IndexerDownloadClientHelpText')}
                  {...downloadClientId}
                  includeAny={true}
                  protocol={protocol.value}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('Tags')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  helpText={translate('IndexerTagSeriesHelpText')}
                  {...tags}
                  onChange={onInputChange}
                />
              </FormGroup>
            </Form>
        }
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteIndexerPress}
            >
              {translate('Delete')}
            </Button>
        }

        <AdvancedSettingsButton
          advancedSettings={advancedSettings}
          onAdvancedSettingsPress={onAdvancedSettingsPress}
          showLabel={false}
        />

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={onTestPress}
        >
          {translate('Test')}
        </SpinnerErrorButton>

        <Button
          onPress={onModalClose}
        >
          {translate('Cancel')}
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditIndexerModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  isTesting: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onAdvancedSettingsPress: PropTypes.func.isRequired,
  onDeleteIndexerPress: PropTypes.func
};

export default EditIndexerModalContent;
