import PropTypes from 'prop-types';
import React, { Component } from 'react';
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
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import styles from './EditDownloadClientModalContent.css';

class EditDownloadClientModalContent extends Component {

  //
  // Render

  render() {
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
      onDeleteDownloadClientPress,
      ...otherProps
    } = this.props;

    const {
      id,
      implementationName,
      name,
      enable,
      protocol,
      priority,
      removeCompletedDownloads,
      removeFailedDownloads,
      fields,
      message
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {`${id ? 'Edit' : 'Add'} Download Client - ${implementationName}`}
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>Unable to add a new download client, please try again.</div>
          }

          {
            !isFetching && !error &&
              <Form {...otherProps}>
                {
                  !!message &&
                    <Alert
                      className={styles.message}
                      kind={message.value.type}
                    >
                      {message.value.message}
                    </Alert>
                }

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
                  <FormLabel>Enable</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="enable"
                    {...enable}
                    onChange={onInputChange}
                  />
                </FormGroup>

                {
                  fields.map((field) => {
                    return (
                      <ProviderFieldFormGroup
                        key={field.name}
                        advancedSettings={advancedSettings}
                        provider="downloadClient"
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
                  <FormLabel>Client Priority</FormLabel>

                  <FormInputGroup
                    type={inputTypes.NUMBER}
                    name="priority"
                    helpText="Prioritize multiple Download Clients. Round-Robin is used for clients with the same priority."
                    min={1}
                    max={50}
                    {...priority}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FieldSet
                  size={sizes.SMALL}
                  legend="Completed Download Handling"
                >
                  <FormGroup>
                    <FormLabel>Remove Completed</FormLabel>

                    <FormInputGroup
                      type={inputTypes.CHECK}
                      name="removeCompletedDownloads"
                      helpText="Remove imported downloads from download client history (when finished seeding for torrents)"
                      {...removeCompletedDownloads}
                      onChange={onInputChange}
                    />
                  </FormGroup>

                  {
                    protocol.value !== 'torrent' &&
                      <FormGroup>
                        <FormLabel>Remove Failed</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="removeFailedDownloads"
                          helpText="Remove failed downloads from download client history"
                          {...removeFailedDownloads}
                          onChange={onInputChange}
                        />
                      </FormGroup>
                  }
                </FieldSet>
              </Form>
          }
        </ModalBody>
        <ModalFooter>
          {
            id &&
              <Button
                className={styles.deleteButton}
                kind={kinds.DANGER}
                onPress={onDeleteDownloadClientPress}
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
}

EditDownloadClientModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  isTesting: PropTypes.bool.isRequired,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onDeleteDownloadClientPress: PropTypes.func
};

export default EditDownloadClientModalContent;
