import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import LanguageProfileItems from './LanguageProfileItems';
import styles from './EditLanguageProfileModalContent.css';

function EditLanguageProfileModalContent(props) {
  const {
    isFetching,
    error,
    isSaving,
    saveError,
    languages,
    item,
    isInUse,
    onInputChange,
    onCutoffChange,
    onSavePress,
    onModalClose,
    onDeleteLanguageProfilePress,
    ...otherProps
  } = props;

  const {
    id,
    name,
    cutoff,
    languages: itemLanguages
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Language Profile' : 'Add Language Profile'}
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to add a new language profile, please try again.</div>
        }

        {
          !isFetching && !error &&
            <Form
              {...otherProps}
            >
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
                <FormLabel>Cutoff</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="cutoff"
                  {...cutoff}
                  value={cutoff ? cutoff.value.id : 0}
                  values={languages}
                  helpText="Once this language is reached Sonarr will no longer download episodes"
                  onChange={onCutoffChange}
                />
              </FormGroup>

              <LanguageProfileItems
                languageProfileItems={itemLanguages.value}
                errors={itemLanguages.errors}
                warnings={itemLanguages.warnings}
                {...otherProps}
              />

            </Form>
        }
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <div
              className={styles.deleteButtonContainer}
              title={isInUse && 'Can\'t delete a language profile that is attached to a series'}
            >
              <Button
                kind={kinds.DANGER}
                isDisabled={isInUse}
                onPress={onDeleteLanguageProfilePress}
              >
                Delete
              </Button>
            </div>
        }

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

EditLanguageProfileModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  item: PropTypes.object.isRequired,
  isInUse: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onCutoffChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteLanguageProfilePress: PropTypes.func
};

export default EditLanguageProfileModalContent;
