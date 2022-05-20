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
    protocol,
    downloadClientId
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {`${id ? 'Edit' : 'Add'} Indexer - ${implementationName}`}
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to add a new indexer, please try again.</div>
        }

        {
          !isFetching && !error &&
            <Form {...otherProps}>
              <FormGroup>
                <FormLabel>Name</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="name"
                  {...name}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Enable RSS</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableRss"
                  helpTextWarning={supportsRss.value ? undefined : 'RSS is not supported with this indexer'}
                  isDisabled={!supportsRss.value}
                  {...enableRss}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Enable Automatic Search</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableAutomaticSearch"
                  helpText={supportsSearch.value ? 'Will be used when automatic searches are performed via the UI or by Sonarr' : undefined}
                  helpTextWarning={supportsSearch.value ? undefined : 'Search is not supported with this indexer'}
                  isDisabled={!supportsSearch.value}
                  {...enableAutomaticSearch}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Enable Interactive Search</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableInteractiveSearch"
                  helpText={supportsSearch.value ? 'Will be used when interactive search is used' : undefined}
                  helpTextWarning={supportsSearch.value ? undefined : 'Search is not supported with this indexer'}
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
                <FormLabel>Indexer Priority</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="priority"
                  helpText="Indexer Priority from 1 (Highest) to 50 (Lowest). Default: 25. Used when grabbing releases as a tiebreaker for otherwise equal releases, Sonarr will still use all enabled indexers for RSS Sync and Searching."
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
                <FormLabel>DownloadClient</FormLabel>

                <FormInputGroup
                  type={inputTypes.DOWNLOAD_CLIENT_SELECT}
                  name="downloadClientId"
                  helpText={'Specify which download client is used for grabs from this indexer'}
                  {...downloadClientId}
                  includeAny={true}
                  protocol={protocol.value}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Tags</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  helpText="Only use this indexer for series with at least one matching tag. Leave blank to use with all series."
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
              Delete
            </Button>
        }

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={onTestPress}
        >
          Test
        </SpinnerErrorButton>

        <Button
          onPress={onModalClose}
        >
          Cancel
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          Save
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
  onDeleteIndexerPress: PropTypes.func
};

export default EditIndexerModalContent;
