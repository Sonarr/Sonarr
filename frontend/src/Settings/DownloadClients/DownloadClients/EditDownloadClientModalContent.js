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
import translate from 'Utilities/String/translate';
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
      tags,
      message
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {id ? translate('EditDownloadClientImplementation', { implementationName }) : translate('AddDownloadClientImplementation', { implementationName })}
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>
                {translate('AddDownloadClientError')}
              </div>
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
                  <FormLabel>{translate('Name')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TEXT}
                    name="name"
                    {...name}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Enable')}</FormLabel>

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
                  <FormLabel>{translate('ClientPriority')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.NUMBER}
                    name="priority"
                    helpText={translate('PriorityHelpText')}
                    min={1}
                    max={50}
                    {...priority}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Tags')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TAG}
                    name="tags"
                    helpText={translate('DownloadClientSeriesTagHelpText')}
                    {...tags}
                    onChange={onInputChange}
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
                      onChange={onInputChange}
                    />
                  </FormGroup>

                  {
                    protocol.value !== 'torrent' &&
                      <FormGroup>
                        <FormLabel>{translate('RemoveFailed')}</FormLabel>

                        <FormInputGroup
                          type={inputTypes.CHECK}
                          name="removeFailedDownloads"
                          helpText={translate('RemoveFailedDownloadsHelpText')}
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
                {translate('Delete')}
              </Button>
          }

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
